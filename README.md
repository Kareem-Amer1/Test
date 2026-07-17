# HireExam

Interview quiz system for evaluating job candidates. HR conducts timed exams on a shared device; MCQ and True/False questions are auto-graded; essay answers are scored manually. Exams are archived permanently.

## Stack

| Layer | Tech |
|---|---|
| Frontend | React 18, Vite, TypeScript, Tailwind, shadcn/ui, TanStack Query |
| Backend | .NET 8 Clean Architecture, MongoDB, JWT (RS256) |
| Tests | xUnit, Vitest, WebApplicationFactory integration tests |

See `inspect.md` for the full plan and `Prompt.md` for product requirements.

## Prerequisites

- Node.js 20+
- .NET 8 SDK
- MongoDB running locally (default `mongodb://localhost:27017`)

## Run locally

### Backend (port 5080)

```bash
cd backend/src/Template.Api
dotnet run
```

Swagger: http://localhost:5080/swagger

Default Super Admin (seeded on first boot):

- Email: `admin@hireexam.local`
- Password: `Admin@12345`

### Frontend

```bash
npm install
npm run dev
```

Set `VITE_API_URL=http://localhost:5080` in `.env`.

App: http://localhost:5173 (or the port Vite prints)

## Tests

```bash
# Backend unit + integration
dotnet test backend/src/Template.Api/Template.Api.sln

# Frontend
npm test
```

Manual E2E checklist: [docs/TESTING.md](docs/TESTING.md)

## Docker

```bash
docker compose up --build
```

API is exposed on port `15219` (see `docker-compose.yml`).

## Project layout

```
backend/src/          .NET API + Application + Core + Infrastructure
src/                  React frontend (features/, pages/, layouts/)
docs/TESTING.md       Manual QA checklist
inspect.md            Architecture & phase plan
```
