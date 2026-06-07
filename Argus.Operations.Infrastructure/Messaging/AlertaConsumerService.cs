using System.Text;
using System.Text.Json;
using Argus.Operations.Application.Messaging;
using Argus.Operations.Domain.Entities;
using Argus.Operations.Domain.Enums;
using Argus.Operations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Argus.Operations.Infrastructure.Messaging;

// Escuta a fila 'argus.alertas' no CloudAMQP. Cada alerta ALTO/CRITICO
// publicado pela API Java vira automaticamente uma Ocorrência operacional,
// atribuída à primeira brigada/brigadista ativos do banco (coordenador
// reatribui depois via PUT /api/ocorrencias/{id}).
//
// Garantias:
//   - Idempotência: se já existe ocorrência pra aquele AlertaId, ack sem duplicar.
//   - Auto-recovery: reconnect automático se a rede cai (config do driver).
//   - Poison message: JSON malformado vai pro DLQ (Nack sem requeue).
//   - Erro transitório (DB caiu): Nack com requeue, mensagem volta pra fila.
public class AlertaConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlertaConsumerService> _logger;
    private readonly string _connectionString;
    private readonly string _queueName;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JavaLocalDateTimeConverter() }
    };

    private IConnection? _connection;
    private IChannel? _channel;

    public AlertaConsumerService(
        IServiceScopeFactory scopeFactory,
        ILogger<AlertaConsumerService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _connectionString = configuration["RabbitMq:ConnectionString"]
            ?? throw new InvalidOperationException(
                "RabbitMq:ConnectionString não configurada. Use user-secrets em dev ou Application Settings na Azure.");
        _queueName = configuration["RabbitMq:QueueName"] ?? "argus.alertas";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Loop externo: se a conexão cair fora do range do auto-recovery do driver,
        // este loop re-tenta com backoff fixo. Sem isso, qualquer falha brusca
        // mata o BackgroundService e a API perderia o consumer até reiniciar.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConectarEConsumirAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consumer caiu. Tentando reconectar em 10s...");
                try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private async Task ConectarEConsumirAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_connectionString),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = await factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        // Processa uma mensagem por vez — facilita defesa de idempotência
        // e evita um alerta novo chegar antes do anterior terminar de persistir.
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: ct);

        _logger.LogInformation("Consumer RabbitMQ conectado em '{Queue}'. Aguardando alertas...", _queueName);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct);

        // Mantém o serviço vivo até cancelamento — o consumer roda em callback.
        await Task.Delay(Timeout.Infinite, ct);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var deliveryTag = ea.DeliveryTag;
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());

        try
        {
            var alerta = JsonSerializer.Deserialize<AlertaQueueDto>(json, JsonOpts);
            if (alerta == null)
            {
                _logger.LogWarning("Mensagem nula/vazia na fila. Descartando sem requeue.");
                await _channel!.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
                return;
            }

            await ProcessarAlertaAsync(alerta);
            await _channel!.BasicAckAsync(deliveryTag, multiple: false);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Mensagem malformada na fila. Descartando sem requeue. Payload: {Json}", json);
            await _channel!.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro transitório processando alerta. Requeueing pra retry.");
            await _channel!.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
        }
    }

    private async Task ProcessarAlertaAsync(AlertaQueueDto alerta)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ArgusDbContext>();

        // Idempotência: alerta entregue 2x não pode virar 2 ocorrências.
        var jaExiste = await db.Ocorrencias.AnyAsync(o => o.AlertaId == alerta.Id);
        if (jaExiste)
        {
            _logger.LogInformation("Alerta {AlertaId} já tem ocorrência. Mensagem ackada sem duplicar.", alerta.Id);
            return;
        }

        // Default: primeira brigada+brigadista ativos do banco. Coordenador
        // reatribui depois se fizer sentido (ex: brigada de outra região).
        var brigada = await db.Brigadas.FirstOrDefaultAsync(b => b.Ativa);
        if (brigada == null)
        {
            _logger.LogWarning("Nenhuma brigada ativa pra atribuir o alerta {AlertaId}. Mensagem ackada (sem ocorrência).", alerta.Id);
            return;
        }

        var brigadista = await db.Brigadistas.FirstOrDefaultAsync(b => b.Ativo && b.BrigadaId == brigada.Id);
        if (brigadista == null)
        {
            _logger.LogWarning("Brigada {BrigadaId} sem brigadista ativo. Alerta {AlertaId} pulado.", brigada.Id, alerta.Id);
            return;
        }

        db.Ocorrencias.Add(new Ocorrencia
        {
            Descricao = MontarDescricao(alerta),
            Latitude = alerta.Latitude,
            Longitude = alerta.Longitude,
            BrigadaId = brigada.Id,
            BrigadistaId = brigadista.Id,
            AlertaId = alerta.Id,
            DataAbertura = DateTime.UtcNow,
            Status = StatusOcorrencia.Aberta
        });

        await db.SaveChangesAsync();
        _logger.LogInformation(
            "Ocorrência criada pra alerta {AlertaId} ({Nivel}) na {RegiaoNome}. Atribuída à brigada {BrigadaId}/brigadista {BrigadistaId}.",
            alerta.Id, alerta.Nivel, alerta.RegiaoNome, brigada.Id, brigadista.Id);
    }

    private static string MontarDescricao(AlertaQueueDto alerta)
    {
        var partes = new List<string> { alerta.Titulo };
        if (!string.IsNullOrWhiteSpace(alerta.Descricao))
            partes.Add(alerta.Descricao);
        if (!string.IsNullOrWhiteSpace(alerta.RecomendacaoOperacional))
            partes.Add($"Recomendação: {alerta.RecomendacaoOperacional}");
        if (!string.IsNullOrWhiteSpace(alerta.RegiaoNome))
            partes.Add($"Região: {alerta.RegiaoNome}");
        return string.Join(" — ", partes);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_channel is not null)
                await _channel.CloseAsync(cancellationToken);
            if (_connection is not null)
                await _connection.CloseAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao fechar conexão RabbitMQ no shutdown (ignorado).");
        }
        await base.StopAsync(cancellationToken);
    }
}
