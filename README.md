# Argus Operations API — DevOps Tools & Cloud Computing

<p align="center">
  <img alt=".NET" src="https://img.shields.io/badge/.NET%209-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white">
  <img alt="ASP.NET Core" src="https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
  <img alt="Oracle" src="https://img.shields.io/badge/Oracle%20Database-F80000?style=for-the-badge&logo=oracle&logoColor=white">
  <img alt="Docker" src="https://img.shields.io/badge/Docker-Container-2496ED?style=for-the-badge&logo=docker&logoColor=white">
  <img alt="Azure DevOps" src="https://img.shields.io/badge/Azure%20DevOps-Boards%20%7C%20Repos%20%7C%20Pipelines-0078D7?style=for-the-badge&logo=azuredevops&logoColor=white">
  <img alt="Azure" src="https://img.shields.io/badge/Azure-ACR%20%2B%20ACI-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white">
</p>

---

## 1. Visão Geral do Projeto

O **Argus Operations API** é uma API REST desenvolvida em **C# com ASP.NET Core .NET 9**, utilizada para apoio operacional no gerenciamento de recursos, equipes e ocorrências relacionadas a operações de combate, prevenção e monitoramento.

Este repositório foi estruturado para a entrega da disciplina **DevOps Tools & Cloud Computing**, utilizando uma solução da disciplina **Advanced Business Development with .NET**.

O foco principal desta entrega é demonstrar um fluxo DevOps completo com:

* Planejamento e rastreabilidade no **Azure Boards**.
* Versionamento Git no **Azure Repos**.
* Integração e entrega contínua com **Azure Pipelines**.
* Infraestrutura provisionada por **Azure CLI**.
* Containerização com **Docker**.
* Publicação da imagem no **Azure Container Registry — ACR**.
* Deploy em nuvem no **Azure Container Instance — ACI**.
* Execução de testes automatizados.
* Publicação de artefatos de build.
* Validação de CRUD em ambiente publicado em nuvem.

---

## 2. Integrantes

| Nome                          |       RM | Responsabilidade                                                         |
| ----------------------------- | -------: | ------------------------------------------------------------------------ |
| Maria Eduarda Araujo Penas    | RM560944 | DevOps, Azure DevOps, Azure Repos, Azure Pipelines, documentação e vídeo |
| Alane Rocha da Silva          | RM561052 | Backend .NET, infraestrutura Azure, banco de dados e deploy              |
| Anna Beatriz de Araujo Bonfim | RM559561 | Apoio em frontend, IA, documentação e validações                         |

---

## 3. Objetivo da Entrega DevOps

O objetivo da entrega é implementar em nuvem uma solução .NET utilizando o ecossistema do **Azure DevOps**, contemplando:

1. Criação de projeto privado no Azure DevOps.
2. Importação do código fonte para o Azure Repos.
3. Criação de tarefas no Azure Boards.
4. Vinculação de branch, commits e Pull Request às tarefas.
5. Proteção da branch principal `main`.
6. Criação de pipeline de Build com testes e artefatos.
7. Criação de fluxo de Release/Deploy automático.
8. Provisionamento da infraestrutura por script Azure CLI.
9. Deploy da API em nuvem utilizando container.
10. Demonstração de operações CRUD em pelo menos duas tabelas.

---

## 4. Stack Técnica

| Camada              | Tecnologia               |
| ------------------- | ------------------------ |
| Linguagem           | C#                       |
| Framework           | ASP.NET Core .NET 9      |
| Arquitetura         | Clean Architecture       |
| API                 | REST JSON                |
| ORM                 | Entity Framework Core    |
| Banco de dados      | Oracle                   |
| Autenticação        | JWT Bearer               |
| Documentação da API | Swagger / OpenAPI        |
| Testes              | xUnit                    |
| Containerização     | Docker                   |
| Registry            | Azure Container Registry |
| Deploy              | Azure Container Instance |
| CI/CD               | Azure Pipelines          |
| Versionamento       | Azure Repos Git          |
| Gestão de tarefas   | Azure Boards             |
| Infraestrutura      | Azure CLI                |

---

## 5. Arquitetura da Aplicação C#/.NET

A aplicação segue uma organização em camadas, baseada em princípios de separação de responsabilidades. Essa estrutura facilita manutenção, testes, evolução da API e integração com banco de dados e serviços externos.

```mermaid
flowchart TD
    Client[Cliente / Swagger / Postman] --> API[Argus.Operations.API]

    API --> Controllers[Controllers REST]
    API --> Auth[Autenticação JWT]
    API --> Swagger[Swagger / OpenAPI]

    Controllers --> Application[Argus.Operations.Application]
    Application --> Domain[Argus.Operations.Domain]

    API --> Infrastructure[Argus.Operations.Infrastructure]
    Infrastructure --> EF[Entity Framework Core]
    EF --> DB[(Oracle Database)]

    Infrastructure --> Services[Serviços de Infraestrutura]
    Services --> Token[Token Service]
    Services --> Security[Password Hasher]
```

### Projetos da Solution

| Projeto                           | Responsabilidade                                                                       |
| --------------------------------- | -------------------------------------------------------------------------------------- |
| `Argus.Operations.API`            | Camada de entrada da aplicação, controllers, autenticação, Swagger e configuração HTTP |
| `Argus.Operations.Application`    | Regras de aplicação, contratos, interfaces e DTOs                                      |
| `Argus.Operations.Domain`         | Entidades, enums e regras de domínio                                                   |
| `Argus.Operations.Infrastructure` | Persistência, Entity Framework, serviços concretos e integrações                       |
| `Argus.Operations.Tests`          | Testes automatizados da aplicação                                                      |

---

## 6. Arquitetura DevOps em Nuvem

A arquitetura DevOps utiliza o Azure DevOps para controlar o ciclo de vida da aplicação e o Azure Cloud para executar a API publicada em container.

```mermaid
flowchart LR
    Dev[Desenvolvedor] --> Boards[Azure Boards]
    Dev --> Repos[Azure Repos]

    Boards --> Tasks[Tasks / Work Items]
    Tasks --> Branch[Feature Branch]

    Repos --> Branch
    Branch --> Commit[Commits vinculados]
    Commit --> PR[Pull Request]

    PR --> Policy[Branch Policy]
    Policy --> Main[Main protegida]

    Main --> Pipeline[Azure Pipelines]

    Pipeline --> CI[Build CI]
    CI --> Restore[dotnet restore]
    Restore --> Build[dotnet build]
    Build --> Test[dotnet test]
    Test --> Artifact[Artefatos]

    Pipeline --> CD[Release CD]
    CD --> Docker[Docker Build]
    Docker --> ACR[Azure Container Registry]
    ACR --> ACI[Azure Container Instance]

    ACI --> API[API .NET em Nuvem]
    API --> Database[(Banco de Dados)]
```

---

## 7. Fluxo DevOps Implementado

```mermaid
sequenceDiagram
    participant Dev as Desenvolvedor
    participant Boards as Azure Boards
    participant Repos as Azure Repos
    participant PR as Pull Request
    participant Pipe as Azure Pipelines
    participant ACR as Azure Container Registry
    participant ACI as Azure Container Instance

    Dev->>Boards: Cria tasks da entrega DevOps
    Dev->>Repos: Cria branch feature/AB1-devops-pipeline
    Dev->>Repos: Realiza commits com referência AB#ID
    Dev->>PR: Abre Pull Request para main
    PR->>PR: Valida Work Items e reviewer
    PR->>Repos: Merge na main
    Repos->>Pipe: Dispara pipeline automaticamente
    Pipe->>Pipe: Restore, build e testes
    Pipe->>Pipe: Publica artefatos e resultados de teste
    Pipe->>ACR: Build e push da imagem Docker
    Pipe->>ACI: Atualiza container em nuvem
    ACI->>Dev: API disponível em endpoint público
```

---

## 8. Azure Boards

O projeto foi organizado no **Azure Boards** com tasks separadas para demonstrar planejamento, rastreabilidade e controle da implementação.

### Tasks criadas

| Work Item | Título                                                               | Objetivo                                                |
| --------- | -------------------------------------------------------------------- | ------------------------------------------------------- |
| AB#1      | Implementar pipeline CI/CD e deploy em nuvem da API Argus Operations | Task principal da entrega DevOps                        |
| AB#2      | Provisionar infraestrutura Azure via Azure CLI                       | Criação dos recursos Azure por script                   |
| AB#3      | Organizar arquivos obrigatórios da entrega DevOps                    | Estrutura de `/scripts`, `/dockerfiles`, `/docs` e YAML |
| AB#4      | Configurar pipeline de build e testes automatizados                  | Restore, build, testes e artefatos                      |
| AB#5      | Configurar deploy automático em nuvem com ACR e ACI                  | Build da imagem, push no ACR e deploy no ACI            |
| AB#6      | Configurar proteção da branch main e fluxo de Pull Request           | Branch policies, reviewer e vínculo com Work Item       |
| AB#7      | Preparar documentação final, arquitetura e roteiro do vídeo          | README, arquitetura, PDF e vídeo                        |

As tasks foram vinculadas ao Pull Request e aos commits por meio da referência `AB#`.

Exemplo de commit vinculado:

```bash
git commit -m "docs: reestrutura readme com foco devops AB#7"
```

---

## 9. Azure Repos

O código fonte foi importado para o **Azure Repos**, mantendo Git como sistema de versionamento.

### Branches principais

| Branch                        | Finalidade                                |
| ----------------------------- | ----------------------------------------- |
| `main`                        | Branch principal protegida                |
| `feature/AB1-devops-pipeline` | Branch de implementação da entrega DevOps |

### Fluxo adotado

1. Código importado para o Azure Repos.
2. Criação de branch de feature.
3. Commits vinculados às tasks do Azure Boards.
4. Abertura de Pull Request para a `main`.
5. Validação por políticas de branch.
6. Merge na `main`.
7. Execução automática da pipeline.

---

## 10. Proteção da Branch Main

A branch `main` foi configurada com políticas para garantir controle e rastreabilidade.

| Política                   | Configuração                                           |
| -------------------------- | ------------------------------------------------------ |
| Pull Request obrigatório   | Sim                                                    |
| Número mínimo de revisores | 1                                                      |
| Revisor padrão             | Usuário RM561052                                       |
| Work Item vinculado        | Obrigatório                                            |
| Commits diretos na main    | Evitados pelo fluxo de PR                              |
| Aprovação própria          | Permitida para simulação acadêmica, conforme enunciado |

Essa configuração garante que alterações relevantes sejam feitas via branch, revisadas e vinculadas a uma tarefa do Azure Boards.

---

## 11. Infraestrutura Azure via Azure CLI

A infraestrutura foi provisionada por script Azure CLI, localizado em:

```txt
/scripts/script-infra-create.sh
```

### Recursos criados

| Recurso                  | Nome                 |
| ------------------------ | -------------------- |
| Resource Group           | `rg-argus-rm561052`  |
| Azure Container Registry | `acrargusrm561052`   |
| Azure Container Instance | `aci-argus-rm561052` |
| DNS Label                | `argus-rm561052-api` |

### Estratégia de deploy

A estratégia utilizada foi:

```txt
Container + Azure Container Registry + Azure Container Instance
```

O **Azure Container Registry** armazena a imagem Docker da API.
O **Azure Container Instance** executa a imagem em nuvem com endpoint público.

---

## 12. Estrutura Obrigatória da Entrega

O repositório foi organizado para atender aos requisitos da disciplina.

```txt
ARGUS-OPERATIONS-DOTNET-DEVOPS
├── Argus.Operations.API
├── Argus.Operations.Application
├── Argus.Operations.Domain
├── Argus.Operations.Infrastructure
├── Argus.Operations.Tests
├── dockerfiles
│   └── Dockerfile
├── scripts
│   ├── script-infra-create.sh
│   ├── script-bd.sql
│   └── seed-dados-teste.sql
├── docs
│   ├── estrutura-entrega.md
│   ├── branch-policy.md
│   └── arquitetura-macro.md
├── azure-pipeline.yml
├── Argus.Operations.sln
└── README.md
```

### Arquivos obrigatórios

| Arquivo                          | Descrição                                               |
| -------------------------------- | ------------------------------------------------------- |
| `scripts/script-infra-create.sh` | Script Azure CLI para provisionamento da infraestrutura |
| `scripts/script-bd.sql`          | DDL dos objetos de banco                                |
| `scripts/seed-dados-teste.sql`   | Dados iniciais para testes                              |
| `dockerfiles/Dockerfile`         | Dockerfile da API .NET                                  |
| `azure-pipeline.yml`             | Pipeline CI/CD do Azure DevOps                          |
| `README.md`                      | Documentação da solução e da entrega DevOps             |

---

## 13. Pipeline CI/CD

A pipeline está definida no arquivo:

```txt
azure-pipeline.yml
```

Ela é acionada automaticamente após alterações na branch `main`.

```mermaid
flowchart TD
    Start[Merge na main] --> Trigger[Trigger automático]
    Trigger --> StageBuild[Stage 1 - Build]
    StageBuild --> Restore[dotnet restore]
    Restore --> Build[dotnet build]
    Build --> Tests[dotnet test]
    Tests --> TestResults[Publicar resultados de teste]
    TestResults --> Artifact[Publicar artefato]

    Artifact --> StageDeploy[Stage 2 - Docker e Deploy]
    StageDeploy --> DockerBuild[Docker build]
    DockerBuild --> PushACR[Push para ACR]
    PushACR --> DeleteACI[Remove ACI anterior]
    DeleteACI --> CreateACI[Cria ACI com imagem latest]
    CreateACI --> Published[API publicada em nuvem]
```

### Stage 1 — Build

O estágio de Build executa:

1. Instalação do SDK .NET 9.
2. Restore das dependências.
3. Build da solution.
4. Execução dos testes automatizados.
5. Publicação dos resultados de teste.
6. Publicação do artefato da API.

### Stage 2 — Docker e Deploy

O estágio de Deploy executa:

1. Login no Azure.
2. Login no Azure Container Registry.
3. Build da imagem Docker.
4. Push da imagem para o ACR.
5. Remoção do Azure Container Instance anterior.
6. Criação de novo Azure Container Instance com a imagem atualizada.
7. Exibição do endpoint público da API.

---

## 14. Docker

A API foi containerizada utilizando o arquivo:

```txt
dockerfiles/Dockerfile
```

### Build local da imagem

```bash
docker build -f dockerfiles/Dockerfile -t argus-operations-api .
```

### Execução local do container

```bash
docker run -p 8080:8080 argus-operations-api
```

Endpoint local:

```txt
http://localhost:8080
```

---

## 15. Variáveis de Ambiente e Segurança

Dados sensíveis não devem ser expostos no código fonte.

A aplicação utiliza variáveis de ambiente para configurações sensíveis, como conexão com banco e autenticação.

Exemplos:

```txt
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=CONFIGURADO_EM_AMBIENTE_SEGURO
Jwt__Key=CONFIGURADO_EM_AMBIENTE_SEGURO
Jwt__Issuer=CONFIGURADO_EM_AMBIENTE_SEGURO
Jwt__Audience=CONFIGURADO_EM_AMBIENTE_SEGURO
```

Na entrega, os dados sensíveis devem ser protegidos por:

* Azure DevOps Service Connection;
* variáveis secretas da pipeline;
* variáveis de ambiente do container;
* configurações seguras fora do repositório.

---

## 16. Banco de Dados

O script DDL dos objetos do banco está versionado em:

```txt
scripts/script-bd.sql
```

O script de carga de dados de teste está em:

```txt
scripts/seed-dados-teste.sql
```

Durante a demonstração, a persistência das operações CRUD deve ser validada diretamente no banco por comandos `SELECT`.

Exemplos:

```sql
SELECT * FROM TB_BRIGADAS;
SELECT * FROM TB_RECURSOS;
```

---

## 17. Endpoints da API

A API expõe endpoints REST em JSON e possui documentação via Swagger.

Swagger local ou publicado:

```txt
/swagger
```

### Autenticação

| Método | Endpoint             | Descrição                             |
| ------ | -------------------- | ------------------------------------- |
| POST   | `/api/auth/login`    | Realiza login e retorna token JWT     |
| POST   | `/api/auth/register` | Registra novo usuário                 |
| GET    | `/api/auth/me`       | Retorna dados do usuário autenticado  |
| PUT    | `/api/auth/me`       | Atualiza dados do usuário autenticado |

### Alertas e Focos

| Método | Endpoint                             | Descrição                          |
| ------ | ------------------------------------ | ---------------------------------- |
| GET    | `/api/alertas`                       | Lista alertas críticos             |
| GET    | `/api/alertas/{id}`                  | Busca alerta por ID                |
| POST   | `/api/alertas/{id}/criar-ocorrencia` | Cria ocorrência a partir de alerta |
| GET    | `/api/focos`                         | Lista focos de calor               |

### Recursos principais

Os recursos seguem o padrão REST:

| Método | Padrão                | Descrição             |
| ------ | --------------------- | --------------------- |
| GET    | `/api/{recurso}`      | Lista registros       |
| GET    | `/api/{recurso}/{id}` | Busca registro por ID |
| POST   | `/api/{recurso}`      | Cria novo registro    |
| PUT    | `/api/{recurso}/{id}` | Atualiza registro     |
| DELETE | `/api/{recurso}/{id}` | Remove registro       |

Recursos principais da API:

```txt
brigadas
brigadistas
recursos
ocorrencias
registroscampo
usuarios
```

---

## 18. Exemplos de CRUD em JSON

Abaixo estão exemplos de payloads utilizados para validação da API. Os nomes dos campos podem ser ajustados conforme os DTOs finais exibidos no Swagger.

---

### CRUD 1 — Brigadas

#### Create — POST `/api/brigadas`

```json
{
  "nome": "Brigada DevOps Azure",
  "baseOperacional": "Base FIAP",
  "cidade": "São Paulo",
  "estado": "SP",
  "ativa": true
}
```

#### Read — GET `/api/brigadas`

```http
GET /api/brigadas
Authorization: Bearer {token}
```

#### Update — PUT `/api/brigadas/{id}`

```json
{
  "id": 1,
  "nome": "Brigada DevOps Azure Atualizada",
  "baseOperacional": "Base FIAP Paulista",
  "cidade": "São Paulo",
  "estado": "SP",
  "ativa": true
}
```

#### Delete — DELETE `/api/brigadas/{id}`

```http
DELETE /api/brigadas/1
Authorization: Bearer {token}
```

#### Validação no banco

```sql
SELECT * FROM TB_BRIGADAS;
```

---

### CRUD 2 — Recursos

#### Create — POST `/api/recursos`

```json
{
  "nome": "Drone de Monitoramento DevOps",
  "tipo": "Drone",
  "quantidade": 2,
  "status": "Disponivel"
}
```

#### Read — GET `/api/recursos`

```http
GET /api/recursos
Authorization: Bearer {token}
```

#### Update — PUT `/api/recursos/{id}`

```json
{
  "id": 1,
  "nome": "Drone de Monitoramento DevOps Atualizado",
  "tipo": "Drone",
  "quantidade": 3,
  "status": "EmOperacao"
}
```

#### Delete — DELETE `/api/recursos/{id}`

```http
DELETE /api/recursos/1
Authorization: Bearer {token}
```

#### Validação no banco

```sql
SELECT * FROM TB_RECURSOS;
```

---

## 19. Execução Local

### Pré-requisitos

* .NET SDK 9
* Git
* Banco Oracle configurado
* Visual Studio Code ou Visual Studio
* Docker, opcional

### Restaurar dependências

```bash
dotnet restore Argus.Operations.sln
```

### Compilar

```bash
dotnet build Argus.Operations.sln
```

### Executar testes

```bash
dotnet test Argus.Operations.sln
```

### Executar API

```bash
dotnet run --project Argus.Operations.API
```

---

## 20. Testes Automatizados

Os testes ficam no projeto:

```txt
Argus.Operations.Tests
```

Execução local:

```bash
dotnet test Argus.Operations.sln
```

Na pipeline, os testes são executados automaticamente e os resultados são publicados no Azure Pipelines.

---

## 21. Evidências que Devem Aparecer no Vídeo

Durante o vídeo demonstrativo, devem ser evidenciados:

| Etapa         | Evidência                                           |
| ------------- | --------------------------------------------------- |
| README        | Solução, conceito e arquitetura                     |
| Azure Portal  | Resource Group, ACR e ACI criados                   |
| Azure Boards  | Tasks criadas e vinculadas                          |
| Azure Repos   | Código fonte importado                              |
| Branch        | Branch `feature/AB1-devops-pipeline`                |
| Pull Request  | PR para a main com Work Items                       |
| Branch Policy | Main protegida com reviewer e Work Item obrigatório |
| Pipeline CI   | Restore, build, testes e artefatos                  |
| Pipeline CD   | Docker build, push no ACR e deploy no ACI           |
| ACR           | Imagem Docker publicada                             |
| ACI           | Container rodando em nuvem                          |
| API           | Endpoint público acessível                          |
| CRUD          | Create, Read, Update e Delete em duas tabelas       |
| Banco         | SELECT comprovando persistência                     |
| Boards final  | Tasks concluídas com links de PR/commits            |

---

## 22. Links da Entrega

| Item                     | Link                          |
| ------------------------ | ----------------------------- |
| Organização Azure DevOps | `INSERIR_LINK_DA_ORGANIZACAO` |
| Projeto Azure DevOps     | `INSERIR_LINK_DO_PROJETO`     |
| Repositório Azure Repos  | `INSERIR_LINK_DO_REPOSITORIO` |
| Pipeline Azure DevOps    | `INSERIR_LINK_DA_PIPELINE`    |
| Endpoint público da API  | `INSERIR_ENDPOINT_DA_API`     |
| Vídeo YouTube            | `INSERIR_LINK_DO_VIDEO`       |

---

## 23. Checklist da Entrega

| Item                                 | Status                      |
| ------------------------------------ | --------------------------- |
| Projeto privado no Azure DevOps      | Concluído                   |
| Código no Azure Repos                | Concluído                   |
| Azure Boards com tasks               | Concluído                   |
| Branch de feature criada             | Concluído                   |
| Pull Request criado                  | Concluído                   |
| Work Items vinculados                | Concluído                   |
| Branch main protegida                | Concluído                   |
| Script Azure CLI em `/scripts`       | Concluído                   |
| Script `script-bd.sql` em `/scripts` | Concluído                   |
| Dockerfile em `/dockerfiles`         | Concluído                   |
| `azure-pipeline.yml` na raiz         | Concluído                   |
| Pipeline de Build                    | Pendente de execução final  |
| Publicação de testes                 | Pendente de execução final  |
| Publicação de artefato               | Pendente de execução final  |
| Deploy automático em ACI             | Pendente de execução final  |
| CRUD em duas tabelas                 | Pendente de validação final |
| SELECT no banco                      | Pendente de validação final |
| Vídeo YouTube                        | Pendente                    |
| PDF final                            | Pendente                    |

---

## 24. Conclusão

Este projeto demonstra a aplicação prática de **DevOps Tools & Cloud Computing** em uma solução real desenvolvida em **C#/.NET**.

A entrega contempla planejamento com Azure Boards, versionamento com Azure Repos, proteção de branch, Pull Request, integração contínua, testes automatizados, publicação de artefatos, containerização com Docker, infraestrutura via Azure CLI e deploy em nuvem com Azure Container Registry e Azure Container Instance.

Com isso, o projeto atende aos principais requisitos da disciplina e demonstra um fluxo completo de desenvolvimento, entrega e publicação de uma API em ambiente cloud.
