# HireExam — Full Project Plan

## Project Identity

- **Name**: HireExam
- **Slug variants**: `HireExam` (C# namespaces), `hire_exam` (MongoDB), `hire-exam` (Docker/JWT)
- **Concept**: A quiz system for evaluating job candidates during interviews, with auto-grading, template management, and permanent exam archival.
- **Project Controller Mode**: FALSE (single-app — no project picker after login)

---

## Tech Stack (Fixed by Template)

| Layer | Technology |
|---|---|
| Frontend | React 18 + Vite + TypeScript + Tailwind + shadcn/ui |
| Routing | react-router-dom |
| Data fetching | @tanstack/react-query |
| Forms | react-hook-form + zod |
| i18n | i18next / react-i18next |
| Auth (client) | useAuth hook + ProtectedRoute |
| Backend | .NET 8 Clean Architecture |
| Database | MongoDB |
| Auth (server) | JWT (RS256) |
| Deployment | Docker + docker-compose |
| Testing | xUnit (backend) + Vitest (frontend) |

---

## Users & Roles

| Role | Permissions |
|---|---|
| **Super Admin** | All HR permissions + manage HR accounts + manage Positions + view ALL exams across all HRs |
| **HR** | Modify any Position's Template + create exams + view own exams only + grade essay questions |

---

## Database Schema (MongoDB Collections)

### 1. `users`

```
{
  _id: ObjectId,
  email: string,
  passwordHash: string,
  fullName: string,
  role: "SuperAdmin" | "HR",
  isActive: boolean,
  createdAt: DateTime,
  createdBy: ObjectId (nullable)
}
```

### 2. `positions`

```
{
  _id: ObjectId,
  name: string,
  description: string (optional),
  isActive: boolean,
  createdAt: DateTime,
  createdBy: ObjectId
}
```

### 3. `templates`

One-to-one with Position. Separated to keep history clean.

```
{
  _id: ObjectId,
  positionId: ObjectId,
  durationMinutes: int,
  questions: [
    {
      id: string (GUID — stable identifier for each question),
      type: "Essay" | "TrueFalse" | "MCQ",
      text: string,
      points: int,
      // TrueFalse only:
      correctAnswer: boolean (nullable),
      // MCQ only:
      choices: [{ id: string, text: string }] (nullable),
      correctChoiceId: string (nullable),
      order: int
    }
  ],
  lastModifiedAt: DateTime,
  lastModifiedBy: ObjectId
}
```

### 4. `exams`

Immutable once submitted. Contains a full snapshot of questions.

```
{
  _id: ObjectId,
  candidateName: string,
  positionId: ObjectId,
  positionName: string (denormalized snapshot),
  conductedBy: ObjectId (HR who created it),
  conductedByName: string (denormalized),
  durationMinutes: int (snapshot from template at creation time),
  startedAt: DateTime,
  submittedAt: DateTime (nullable — null if in progress),
  status: "InProgress" | "Submitted" | "Graded",
  questionsSnapshot: [
    {
      id: string,
      type: "Essay" | "TrueFalse" | "MCQ",
      text: string,
      points: int,
      choices: [{ id: string, text: string }] (nullable, MCQ only),
      correctAnswer: boolean (nullable, TrueFalse — server-only, never sent to candidate),
      correctChoiceId: string (nullable, MCQ — server-only, never sent to candidate)
    }
  ],
  answers: [
    {
      questionId: string,
      // Essay:
      essayText: string (nullable),
      // TrueFalse:
      trueFalseAnswer: boolean (nullable),
      // MCQ:
      selectedChoiceId: string (nullable)
    }
  ],
  scores: [
    {
      questionId: string,
      earnedPoints: int,
      isAutoGraded: boolean
    }
  ],
  totalScore: int (nullable — null until fully graded),
  maxScore: int,
  autoGradedScore: int,
  isFullyGraded: boolean
}
```

### Relationships Diagram

```
users (1) ──────── creates ──────── (many) exams
positions (1) ──── has one ──────── (1) templates
positions (1) ──── referenced by ── (many) exams
exams contain a SNAPSHOT of the template questions at creation time
```

---

## Project Architecture

```
backend/src/
├── HireExam.Core/
│   ├── Entities/         (User, Position, Template, Exam)
│   └── Interfaces/       (IUserRepository, IPositionRepository, etc.)
├── HireExam.Application/
│   ├── Services/         (AuthService, PositionService, TemplateService, ExamService)
│   └── Common/           (Result<T>, errors)
├── HireExam.Contracts/
│   └── DTOs/             (Request/Response DTOs for each resource)
├── HireExam.Infrastructure/
│   ├── Repositories/     (MongoUserRepository, MongoPositionRepository, etc.)
│   ├── Persistence/      (MongoDbContext, Indexes)
│   ├── Mapping/          (AutoMapper profiles)
│   └── Auth/             (JwtTokenService)
└── HireExam.Api/
    ├── Controllers/      (AuthController, UsersController, PositionsController, TemplatesController, ExamsController)
    └── Middleware/        (ErrorHandling, Auth)

src/ (frontend)
├── features/
│   ├── auth/             (login page, useAuth)
│   ├── dashboard/        (landing page with stats)
│   ├── users/            (Super Admin: manage HR accounts)
│   ├── positions/        (manage positions + templates)
│   ├── exams/            (create exam, exam list, exam detail/grading)
│   └── exam-session/     (candidate-facing exam UI with timer)
└── components/           (shared UI components)
```

---

## Complete Feature List

### Authentication & Authorization
1. Login (email + password)
2. JWT token issuance and refresh
3. Role-based access control (SuperAdmin / HR)
4. Protected routes on frontend

### User Management (Super Admin only)
5. List all HR accounts
6. Create new HR account
7. Delete/deactivate HR account

### Position Management (Super Admin only)
8. List all Positions
9. Create new Position
10. Delete Position (with validation — handle exams referencing it)

### Template Management (Any authenticated user)
11. View Template for a Position (questions + duration)
12. Update exam duration
13. Add a question (Essay / TrueFalse / MCQ) with all details
14. Delete a question from the Template
15. Reorder questions

### Exam Lifecycle
16. Create a new exam (select Position + enter candidate name)
17. Load exam session (candidate-facing, with timer countdown)
18. Auto-submit on timer expiry
19. Manual submit before time runs out
20. Auto-grade MCQ and TrueFalse questions upon submission
21. Calculate auto-graded score

### Exam Review & Grading (HR / Super Admin)
22. List exams (HR: own only, Admin: all — filterable by Position, status, date)
23. View exam detail with full review:
    - Candidate's answer for each question
    - Whether the answer is correct or wrong (for MCQ & TrueFalse)
    - The correct answer displayed alongside wrong answers
    - Score per question
24. Grade essay questions (assign score per question)
25. Finalize grading (mark exam as fully graded, compute total)

### Data Seeding
26. Seed endpoint/service for predefined Positions + Templates (Software Engineer, Sales, IT Support, etc.)

### Dashboard
27. Summary stats (total exams, pending grading, exams per position)

---

## API Endpoints

### Auth

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/login` | Login, returns JWT |
| GET | `/api/auth/me` | Get current user profile |

### Users (Super Admin only)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/users` | List all HR users |
| POST | `/api/users` | Create HR account |
| DELETE | `/api/users/{id}` | Deactivate HR account |

### Positions (Super Admin to create/delete, all auth users to list)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/positions` | List all positions |
| POST | `/api/positions` | Create position (Super Admin) |
| DELETE | `/api/positions/{id}` | Delete position (Super Admin) |

### Templates

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/positions/{positionId}/template` | Get template for a position |
| PUT | `/api/positions/{positionId}/template/duration` | Update exam duration |
| POST | `/api/positions/{positionId}/template/questions` | Add a question |
| PUT | `/api/positions/{positionId}/template/questions/{questionId}` | Update a question |
| DELETE | `/api/positions/{positionId}/template/questions/{questionId}` | Delete a question |
| PUT | `/api/positions/{positionId}/template/questions/reorder` | Reorder questions |

### Exams

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/exams` | List exams (HR: own only, Admin: all) |
| POST | `/api/exams` | Create a new exam (starts session) |
| GET | `/api/exams/{id}` | Get exam detail with full review (answers + correct answers + scores) |
| GET | `/api/exams/{id}/session` | Get exam session for candidate (NO correct answers) |
| PUT | `/api/exams/{id}/answers` | Save candidate answers (during exam) |
| POST | `/api/exams/{id}/submit` | Submit exam (triggers auto-grading) |
| PUT | `/api/exams/{id}/grade` | Grade essay questions (HR) |

### Seed (Super Admin / development)

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/seed` | Seed default positions + templates |

### Dashboard

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/dashboard/stats` | Get summary statistics |

---

## Exam Review Detail — Response Structure

When HR/Admin opens an exam after submission, each question shows:

```csharp
public class ExamAnswerReviewDto
{
    public string QuestionId { get; set; }
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }  // Essay, TrueFalse, MCQ
    public int Points { get; set; }

    // MCQ: the choices list
    public List<ChoiceDto> Choices { get; set; }

    // Candidate's answer
    public string CandidateAnswer { get; set; }  // text, true/false, or choiceId

    // The correct answer (shown only after submission, only to HR/Admin)
    public string CorrectAnswer { get; set; }

    // Was the answer correct or wrong?
    public bool? IsCorrect { get; set; }  // null for Essay questions

    // Score
    public int? EarnedPoints { get; set; }  // null if essay not yet graded
}
```

### Security Rule — Two Different Views:

| Endpoint | Who sees it | What is sent |
|---|---|---|
| `GET /api/exams/{id}/session` | Candidate during exam | Questions only — **NO correct answers ever** |
| `GET /api/exams/{id}` | HR / Admin after submission | Questions + candidate answer + **correct answer** + `isCorrect` flag + score |

---

## Security Considerations

- Correct answers (`correctAnswer`, `correctChoiceId`) are **NEVER** sent to the browser during an exam session
- Auto-grading happens entirely server-side upon submission
- Exams are immutable — no delete endpoint exists
- HR can only access their own exams (enforced in the service layer via `conductedBy` filter)
- Super Admin bypass: service layer checks role and skips ownership filter for SuperAdmin

---

## Project Phases

### Phase 1 — Foundation & Auth
- Template cleanup (remove old project artifacts, rename to HireExam)
- Set `VITE_PROJECT_CONTROLLER_MODE=false`
- User entity, repository, auth service, JWT setup
- Login page, protected routes, role-based guards
- Seed Super Admin account

### Phase 2 — Position & Template Management
- Position entity + CRUD (Super Admin)
- Template entity + question management (all auth users)
- Frontend pages: Positions list, Template editor (add/remove/reorder questions, set duration)
- Seed default positions with templates

### Phase 3 — Exam Creation & Session
- Exam entity + creation flow (snapshot template at creation time)
- Candidate-facing exam session UI (timer, question navigation, answer inputs)
- Auto-submit on timer expiry
- Manual submit

### Phase 4 — Auto-Grading & Review
- Server-side auto-grading logic upon submission
- Exam list page (HR: own exams, Admin: all exams)
- Exam detail/review page:
  - Show candidate's answer per question
  - Show correct/wrong status for MCQ & TrueFalse
  - Show the correct answer when candidate was wrong
  - Show scores
- Essay grading UI (HR assigns scores)
- Total score calculation

### Phase 5 — User Management & Dashboard
- HR account CRUD (Super Admin)
- Dashboard page with stats
- Filtering/search on exam list

### Phase 6 — Polish & Testing
- End-to-end flow testing
- xUnit tests for grading logic and authorization
- Vitest tests for timer logic and exam session
- RTL/i18n verification
- Final cleanup and documentation
