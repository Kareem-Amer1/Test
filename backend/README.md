# HireExam API

ASP.NET Core 8 backend for **HireExam** — Clean Architecture, MongoDB, RS256 JWT, `Result<T>` responses, JSON localization (`ar`/`en`).

## Layers

```
Template.Api             Controllers, middleware, Swagger
Template.Application     Services, validation, Result<T>
Template.Core            Entities, repository interfaces
Template.Infrastructure  MongoDB repos, JWT, AutoMapper, seed
Template.Contracts       DTOs + ApiResponse<T>
```

## Run

```bash
cd backend/src/Template.Api
dotnet run
```

Listens on http://localhost:5080 (see `Properties/launchSettings.json`).

## Configuration (`appsettings.json`)

| Key | Purpose |
|---|---|
| `MongoDB:ConnectionString` | Mongo connection |
| `MongoDB:Databases:Default` | Database name (`hire_exam`) |
| `Jwt:*` | RS256 issuer, audience, keys |
| `Seed:SuperAdminEmail` | First-boot admin account |
| `Cors:AllowedOrigins` | Frontend origins |

## Tests

```bash
dotnet test Template.Api.sln
```

Integration tests use `ASPNETCORE_ENVIRONMENT=Testing` and skip Mongo seed/index boot.

## API surface (v1)

| Area | Base route |
|---|---|
| Auth | `/api/v1/auth` |
| Positions & templates | `/api/v1/positions` |
| Exams | `/api/v1/exams` |
| Users (Super Admin) | `/api/v1/users` |
| Dashboard | `/api/v1/dashboard` |
| Seed | `/api/v1/seed` |
