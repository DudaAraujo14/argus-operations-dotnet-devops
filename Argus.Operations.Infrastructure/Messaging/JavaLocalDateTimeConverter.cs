using System.Text.Json;
using System.Text.Json.Serialization;

namespace Argus.Operations.Infrastructure.Messaging;

// Jackson (serializer padrão do Spring) por padrão emite java.time.LocalDateTime
// como array de inteiros: [year, month, day, hour, minute, second, nano].
// Esse converter aceita esse formato E o ISO 8601 (string), pra não depender
// de a API Java estar com JavaTimeModule + WRITE_DATES_AS_TIMESTAMPS=false.
public class JavaLocalDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return reader.GetDateTime();

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var partes = new List<int>(7);
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                partes.Add(reader.GetInt32());

            // Array pode vir com 5, 6 ou 7 elementos dependendo se houve precisão.
            // Preenchemos com defaults seguros.
            var year = partes.Count > 0 ? partes[0] : 1;
            var month = partes.Count > 1 ? partes[1] : 1;
            var day = partes.Count > 2 ? partes[2] : 1;
            var hour = partes.Count > 3 ? partes[3] : 0;
            var minute = partes.Count > 4 ? partes[4] : 0;
            var second = partes.Count > 5 ? partes[5] : 0;
            // nano (nanossegundos) -> milissegundos. DateTime do .NET só vai
            // até microssegundos com precisão; perder os últimos 6 dígitos é OK
            // pro nosso caso de uso (timestamp operacional, não criptográfico).
            var nano = partes.Count > 6 ? partes[6] : 0;
            var millis = nano / 1_000_000;

            return new DateTime(year, month, day, hour, minute, second, millis, DateTimeKind.Unspecified);
        }

        throw new JsonException($"Não consegui converter DateTime a partir de token '{reader.TokenType}'. Esperado string ISO 8601 ou array Java.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O"));
    }
}
