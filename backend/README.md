# VoiceFlow Studio API (Backend)

ASP.NET Core 8 backend implementing the **VoiceFlow Studio API Constitution**
(Onion Architecture, MongoDB persistence, RS256 JWT auth, Result<T>, AutoMapper,
global rate limiting, Polly resilience, JSON-based localization for `ar`/`en`).

## Layers

```
VoiceFlowStudio.Api            -> Controllers, middleware, localization, Swagger
VoiceFlowStudio.Infrastructure -> MongoDB repos, JWT signer, AutoMapper profiles
VoiceFlowStudio.Application    -> Services, Result<T>, use-case logic
VoiceFlowStudio.Core           -> Entities, repository interfaces (pure C#)
VoiceFlowStudio.Contracts      -> DTOs + ApiResponse<T> envelope
```

Dependency flow is strictly inward: `Api -> Application + Infrastructure + Contracts`,
`Infrastructure -> Application + Core + Contracts`, `Application -> Core + Contracts`,
`Core -> (nothing)`, `Contracts -> (nothing)`.

## Run

```bash
cd backend
dotnet restore
dotnet run --project src/VoiceFlowStudio.Api
```

## Required environment variables / `appsettings.json` keys

| Key | Description |
| --- | --- |
| `Mongo__ConnectionString` | MongoDB connection string |
| `Mongo__Databases__Default` | Default database name |
| `Jwt__PrivateKeyPem` | RS256 private key (PEM) — secrets manager only |
| `Jwt__PublicKeyPem`  | RS256 public key (PEM) |
| `Jwt__Issuer`        | Token issuer |
| `Jwt__Audience`      | Token audience |
| `Jwt__AccessTokenMinutes` | default `60` |
| `Jwt__RefreshTokenDays`   | default `30` |

Generate a dev RS256 keypair:

```bash
openssl genpkey -algorithm RSA -out jwt.key -pkeyopt rsa_keygen_bits:2048
openssl rsa -in jwt.key -pubout -out jwt.pub
```

## Frontend contract

Endpoints exposed under `/api/v1/...` to match the existing Vite frontend's
`src/lib/apiClient.ts`:

| Method | Path | Auth | Notes |
| --- | --- | --- | --- |
| POST | `/api/v1/auth/register` | anon | `{ email, password }` -> `{ accessToken, refreshToken }` |
| POST | `/api/v1/auth/login`    | anon | `{ email, password }` |
| POST | `/api/v1/auth/refresh`  | anon | `{ refreshToken }` |
| POST | `/api/v1/auth/logout`   | bearer | revokes refresh token |
| GET  | `/api/v1/projects`      | bearer | list current user's projects |
| POST | `/api/v1/projects`      | bearer | create |
| GET  | `/api/v1/projects/{id}` | bearer | get one |
| PATCH| `/api/v1/projects/{id}` | bearer | update |
| DELETE | `/api/v1/projects/{id}` | bearer | delete |

All responses use the envelope `{ success, data, message, errors }`.

## Tests

```bash
dotnet test
```

xUnit + Moq. Each Application service has unit tests covering success + failure paths.
