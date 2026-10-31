# fcg-users-api

Microsservico de **Usuarios**: cadastro, autenticacao (emissao de token JWT) e autorizacao de usuarios. Publica `UserCreatedEvent`.

Parte da plataforma **FIAP Cloud Games (FCG) - Fase 2**. Os contratos de integracao (eventos, JWT, convencoes) sao definidos no repositorio [`fcg-contracts`](https://github.com/sampaiobrenner/fcg-contracts) e a orquestracao (docker-compose e guia de Kubernetes) no [`fcg-orchestration`](https://github.com/sampaiobrenner/fcg-orchestration).

## Finalidade

Contexto de **identidade e acesso** extraido do monolito da Fase 1: e o unico servico que conhece senhas e o unico que **emite** o JWT. Catalog, Payments e Notifications apenas validam o token (mesma chave, issuer `fcg-users-api`, audience `fcg`). A biblioteca de jogos ficou na CatalogAPI.

- Cadastro de usuarios (nome, e-mail e senha segura) com e-mail unico.
- Login com emissao de JWT HS256 com as claims do contrato: `sub` (UserId), `email`, `name`, `role` (`User` | `Administrator`) e `jti`.
- Perfil do usuario autenticado e administracao de usuarios (listar, consultar, renomear, alterar papel e desativar) restrita ao papel `Administrator`.
- Publica `UserCreatedEvent` a cada usuario criado para a NotificationsAPI (boas-vindas e projecao local de nome/e-mail).

## Eventos

| Direcao | Evento (`Fcg.Contracts.Events.V1`) | Exchange | Fila | Quando |
|---|---|---|---|---|
| Publica | `UserCreatedEvent(EventId, OccurredAt, UserId, Name, Email)` | `fcg.user-created.v1` | `notifications.user-created` (criada pela NotificationsAPI) | Cadastro (`POST /api/v1/auth/register`) e criacao do administrador pelo seed |
| Consome | - | - | - | A UsersAPI nao consome eventos |

A publicacao usa o **EF Core Outbox** do MassTransit: o evento e gravado em `outbox_message` na mesma transacao do usuario e entregue ao RabbitMQ pelo servico de entrega do outbox. Com o broker fora do ar o cadastro continua respondendo `201` e o evento sai quando o broker volta. `EventId` e um `Guid` v7 novo a cada publicacao e `OccurredAt` e UTC. Formato de referencia: [`samples/user-created.json`](https://github.com/sampaiobrenner/fcg-contracts/blob/main/samples/user-created.json).

## Endpoints

| Metodo e rota | Autorizacao | Request | Respostas |
|---|---|---|---|
| `POST /api/v1/auth/register` | anonimo | `{ "name", "email", "password" }` | `201` `UserResponse` + `Location: /api/v1/users/{id}`; `400` validacao; `409` e-mail ja cadastrado |
| `POST /api/v1/auth/login` | anonimo | `{ "email", "password" }` | `200` `{ "accessToken", "expiresAtUtc", "userId", "email", "role" }`; `400`; `401` credenciais invalidas ou usuario inativo |
| `GET /api/v1/users/me` | autenticado | - | `200` `UserResponse`; `401` |
| `GET /api/v1/users` | `Administrator` | - | `200` `UserResponse[]`; `401`; `403` |
| `GET /api/v1/users/{id}` | `Administrator` | - | `200` `UserResponse`; `401`; `403`; `404` |
| `PUT /api/v1/users/{id}` | `Administrator` | `{ "name" }` | `200` `UserResponse`; `400`; `401`; `403`; `404` |
| `PUT /api/v1/users/{id}/role` | `Administrator` | `{ "role": "User" \| "Administrator" }` | `200` `UserResponse`; `400`; `401`; `403`; `404` |
| `DELETE /api/v1/users/{id}` | `Administrator` | - | `204` (desativa, nao apaga); `401`; `403`; `404` |

`UserResponse = { "id", "name", "email", "role", "active", "createdAt" }`. Erros em `ProblemDetails` (RFC 7807).

Regras de validacao: nome obrigatorio (max. 120); e-mail valido (max. 320, normalizado em minusculas); senha com no minimo 8 caracteres contendo letra, numero e caractere especial. O login responde sempre a mesma mensagem generica (`Credenciais invalidas`) para e-mail inexistente ou senha errada.

### Administrador padrao

Apos aplicar as migrations no startup (`Database__ApplyMigrationsOnStartup=true`), o servico garante um administrador (seed idempotente, mesmo do monolito):

| Nome | E-mail | Senha |
|---|---|---|
| `Administrador FCG` | `admin@cloudgames.com` | `Admin@123` |

Se o administrador ja existir nada e feito; quando ele e criado, o `UserCreatedEvent` correspondente tambem e publicado.

### Exemplo

```bash
curl -s -X POST http://localhost:5101/api/v1/auth/register -H 'Content-Type: application/json' \
  -d '{"name":"Ada Lovelace","email":"ada@fcg.dev","password":"Str0ng!Pass"}'

TOKEN=$(curl -s -X POST http://localhost:5101/api/v1/auth/login -H 'Content-Type: application/json' \
  -d '{"email":"ada@fcg.dev","password":"Str0ng!Pass"}' | jq -r .accessToken)

curl -s http://localhost:5101/api/v1/users/me -H "Authorization: Bearer $TOKEN"
```

## Stack

.NET 10 · Minimal API · MediatR 12 · FluentValidation · EF Core 10 + PostgreSQL · MassTransit 8 + RabbitMQ (EF Core Outbox/Inbox) · JWT · Serilog · OpenAPI + Scalar · xUnit.

## Arquitetura

Camadas com dependencia estritamente para dentro: `WebApi -> Infrastructure -> Application -> Domain`.

```
src/
  Fcg.Users.Domain/           Regras de negocio. Organizado por agregado.
    _Shared/                    Modules, DomainModelBase/PersistenceModelBase, IDomainEvent, BusinessException
  Fcg.Users.Application/      Casos de uso (MediatR). Organizado por <Agregado>/<CasoDeUso>/.
    _Shared/                    IUsersDbContext (leitura), Parser<TIn,TOut>, IIntegrationEventPublisher, ValidationBehavior
  Fcg.Users.Infrastructure/   Portas de saida: DbContext, repositorios, publicacao de eventos. Organizado por agregado.
    _Shared/                    UsersDbContext (+ tabelas de outbox/inbox), MassTransitIntegrationEventPublisher
  Fcg.Users.WebApi/           Portas de entrada: endpoints REST e consumers. Organizado por <Agregado>/<CasoDeUso>/.
    _Shared/                    Endpoints (IEndpoint), erros (ProblemDetails), JWT, mensageria, health checks, migrations
test/
  Fcg.Users.UnitTests/        Espelha src/ 1:1
  Fcg.Users.IntegrationTests/ WebApplicationFactory + PostgreSQL (Testcontainers) + MassTransit test harness
```

Diretrizes:

- **Um caso de uso = uma pasta** com `Command`/`Query`, `Handler`, `Validator`, `Parser` e, na WebApi, o `Endpoint` (implementa `IEndpoint`) ou o `Consumer`.
- **Escrita:** handler -> servico/modelo de dominio -> repositorio do agregado (recebe e devolve o modelo de dominio).
- **Leitura:** handler projeta direto de `IUsersDbContext.DataSet<T>()`, sem repositorio.
- **Dominio:** `DomainModel` (comportamento) envolve o `PersistenceModel` (mapeado pelo EF via `IEntityTypeConfiguration<T>`, descoberto automaticamente).
- **Mapeamento:** `Parser<TIn,TOut>` explicito, sem AutoMapper.
- **Mensagens de erro:** sempre em `Properties/*Resources.resx` da propria camada, nunca string literal.
- **DI:** cada camada registra seus servicos no seu `FcgUsers<Camada>Module`; `Program.cs` apenas compoe os 4 modulos.
- **Eventos de integracao:** publicados via `IIntegrationEventPublisher` antes do `SaveChangesAsync` - o outbox grava na mesma transacao.
- **Consumers:** ficam na WebApi, delegam para um `Command` via MediatR e sao idempotentes (inbox do MassTransit). Cada consumer tem um `ConsumerDefinition` com `EndpointName` igual a constante de `Fcg.Contracts.Messaging.QueueNames`.
- **Contratos:** eventos, filas, claims e papeis vem do pacote `PosTech.Fiap.CloudGames.Contracts`; nunca redefina localmente.
- **Usuario autenticado:** injete `ICurrentUser` (Application/_Shared/Security), que le as claims `sub`, `email`, `name` e `role` do token.
- **Erros HTTP:** `ValidationException` 400, `UnauthorizedException` 401, `ForbiddenException` 403, `NotFoundException` 404, `ConflictException` 409, `BusinessException` 422.
- **JSON:** camelCase e enums como string.

Exemplo de caso de uso:

```
Application/Games/CreateGame/CreateGameCommand.cs
Application/Games/CreateGame/CreateGameCommandHandler.cs
Application/Games/CreateGame/CreateGameCommandValidator.cs
Application/Games/CreateGame/CreateGameResponseParser.cs
WebApi/Games/CreateGame/CreateGameEndpoint.cs
```

## Executando localmente

Pre-requisitos: .NET SDK 10 e a infraestrutura do [`fcg-orchestration`](https://github.com/sampaiobrenner/fcg-orchestration) (`docker compose up -d`).

```bash
dotnet run --project src/Fcg.Users.WebApi
```

- API: http://localhost:5101
- Documentacao (Scalar): http://localhost:5101/scalar/v1
- Health: `/health/live` e `/health/ready`

Migrations (as pendentes sao aplicadas no startup quando `Database__ApplyMigrationsOnStartup=true`):

```bash
dotnet tool restore
dotnet ef migrations add <Nome> -p src/Fcg.Users.Infrastructure -s src/Fcg.Users.WebApi -o _Shared/Context/Migrations
```

## Testes

```bash
dotnet test
```

- `Fcg.Users.UnitTests`: dominio (`Email`, `Password`, `User`), hasher BCrypt, gerador de JWT (claims, issuer, audience, expiracao) e handlers (Moq / EF Core InMemory).
- `Fcg.Users.IntegrationTests`: sobem a API com `WebApplicationFactory` contra um **PostgreSQL real via Testcontainers** (migrations e seed aplicados no startup) e o RabbitMQ substituido pelo `ITestHarness` do MassTransit com o mesmo EF Outbox da aplicacao. Cobrem o fluxo register -> login -> `/users/me`, `403` para `User` nos endpoints de administracao e a publicacao do `UserCreatedEvent` (comparado com `samples/user-created.json`). **Requerem Docker** em execucao.

## CI/CD

Workflow unico [`ci.yml`](.github/workflows/ci.yml):

| Job | Quando | O que faz |
|---|---|---|
| `build-test` | PR, push na `main`, tags `v*` | restore, build, testes com cobertura e analise no SonarCloud (projeto `sampaiobrenner_fcg-users-api`, requer o secret `SONAR_TOKEN`) |
| `docker` | PR | build da imagem |
| `publish` | push na `main` e tags `v*`, apos os testes | publica `ghcr.io/sampaiobrenner/fcg-users-api` (`latest`, `sha-<commit>`, semver) usando o [`docker-publish.yml`](https://github.com/sampaiobrenner/fcg-orchestration/blob/main/.github/workflows/docker-publish.yml) do `fcg-orchestration` |

## Docker

```bash
docker build -t ghcr.io/sampaiobrenner/fcg-users-api:latest .

docker run --rm -p 5101:8080 \
  -e ConnectionStrings__Default="Host=host.docker.internal;Port=5432;Database=fcg_users;Username=fcg;Password=fcg" \
  -e RabbitMq__Host=host.docker.internal -e RabbitMq__Username=fcg -e RabbitMq__Password=fcg \
  -e Jwt__Key=fcg-dev-only-signing-key-change-me-0123456789 \
  ghcr.io/sampaiobrenner/fcg-users-api:latest
```

Imagem multi-stage (`sdk:10.0` restore + publish `-c Release` -> `aspnet:10.0`), usuario non-root (`USER $APP_UID`), porta `8080` (`ASPNETCORE_HTTP_PORTS=8080`). O `.dockerignore` exclui `bin/`, `obj/`, `test/` e `.git/`.

## Kubernetes

Manifests em [`k8s/`](k8s):

| Arquivo | Conteudo |
|---|---|
| `deployment.yaml` | `Deployment` `users-api` com 2 replicas, `envFrom` ConfigMap + Secret, `startupProbe`/`livenessProbe` em `/health/live`, `readinessProbe` em `/health/ready`, requests `100m/128Mi` e limits `500m/256Mi` |
| `service.yaml` | `Service` `ClusterIP` `users-api`, porta `80` -> `http` (8080) |
| `configmap.yaml` | `users-api-config`: configuracoes nao sensiveis (inclui `Jwt__ExpirationMinutes`) |
| `secret.yaml` | `users-api-secret`: connection string, credenciais do RabbitMQ e chave JWT (valores de desenvolvimento) |

```bash
docker build -t ghcr.io/sampaiobrenner/fcg-users-api:latest .
kind load docker-image ghcr.io/sampaiobrenner/fcg-users-api:latest   # ou: minikube image load
kubectl apply -f k8s/
kubectl get pods -l app=users-api
kubectl port-forward svc/users-api 8080:80 && curl localhost:8080/health/ready
```

As migrations rodam no startup (`Database__ApplyMigrationsOnStartup=true`); o EF Core 10 usa lock de migracao e o seed tolera a criacao concorrente do administrador, entao as 2 replicas podem subir juntas. O `/health/ready` so fica `Healthy` com Postgres e RabbitMQ no cluster (ver [`fcg-orchestration`](https://github.com/sampaiobrenner/fcg-orchestration)).

## Variaveis de ambiente

| Variavel | Origem K8s | Descricao | Exemplo |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | ConfigMap | Ambiente | `Production` |
| `ASPNETCORE_HTTP_PORTS` | Dockerfile | Porta HTTP | `8080` |
| `ConnectionStrings__Default` | Secret | Conexao PostgreSQL | `Host=postgres;Port=5432;Database=fcg_users;Username=fcg;Password=***` |
| `Database__ApplyMigrationsOnStartup` | ConfigMap | Aplica migrations e o seed do administrador padrao no startup | `true` |
| `RabbitMq__Host` | ConfigMap | Host do RabbitMQ | `rabbitmq` |
| `RabbitMq__VirtualHost` | ConfigMap | Virtual host | `/` |
| `RabbitMq__Username` | Secret | Usuario do RabbitMQ | `fcg` |
| `RabbitMq__Password` | Secret | Senha do RabbitMQ | `***` |
| `Jwt__Issuer` | ConfigMap | Emissor do token | `fcg-users-api` |
| `Jwt__Audience` | ConfigMap | Audiencia do token | `fcg` |
| `Jwt__Key` | Secret | Chave simetrica (min. 32 caracteres, igual em todos os servicos) | `***` |
| `Jwt__ExpirationMinutes` | ConfigMap | Validade do token emitido no login, em minutos (default `60`) | `60` |

Nenhum dado sensivel fica no ConfigMap. A UsersAPI nao consome filas, portanto nao possui variaveis `Messaging__Queues__*`.
