# Project Template — Constitution

This repository is a **reusable starting point**, not a finished product. Each
real project begins the same way: the user pastes a natural-language spec
("PROJECT / USERS & ROLES / CORE CONCEPT / DATA MODEL / ACCEPTANCE CRITERIA"
style, or any similar free-form brief) describing a *different* app. Your job
when that happens is to turn this template into that app, using the rules
below — without being told the architecture again every time.

This file is the standing contract. Read it before touching anything when a
new project brief arrives. It is intentionally generic — it is not about any
one project (e.g. "Graduate Assessment") and must not be edited to describe a
specific project's domain. Domain-specific notes belong in that project's own
docs, not here.

---

## 1. What this template actually is

Two things ship together and must both be used:

- **Frontend**: React 18 + Vite + TypeScript + Tailwind + shadcn/ui, in `src/`.
  Routing via `react-router-dom` (`src/App.tsx`), data fetching via
  `@tanstack/react-query`, forms via `react-hook-form` + `zod`, i18n via
  `i18next`/`react-i18next`, auth state via `src/hooks/useAuth.tsx` +
  `src/components/ProtectedRoute.tsx`, API access via `src/lib/apiClient.ts`.
- **Backend**: .NET 8 Clean Architecture under `backend/src/`:
  `Template.Core` (entities, repository interfaces) →
  `Template.Application` (services, business logic, `Result<T>` pattern) →
  `Template.Contracts` (DTOs shared with the API) →
  `Template.Infrastructure` (MongoDB repositories, JWT issuing, AutoMapper
  profiles) → `Template.Api` (controllers, DI wiring, middleware). Auth is
  JWT (RS256), persistence is MongoDB, deployment is Docker
  (`backend/Dockerfile`, `docker-compose.yml`).

**Rule: the .NET + MongoDB backend is the backend for every project built
from this template, full stop.** If a pasted spec says "use Supabase" (or
Firebase, or any other BaaS) for the database/auth/edge-functions, treat that
as generic shorthand for "a backend with a database and server-side auth" —
implement it with this template's actual backend, not the named product.
Do not add a Supabase project, Supabase tables, or Supabase edge functions.
The `@supabase/supabase-js` dependency and `src/integrations/supabase` folder
are legacy from a prior project built on this template; don't extend them,
and remove them during cleanup (§3) unless a specific brief explicitly asks
for Supabase by name after you've flagged the mismatch and the user confirms.

### Mapping common spec vocabulary onto this stack

| Spec says... | Build it as... |
|---|---|
| "Supabase table `x`" / "Postgres table" | A `Template.Core.Entities` class + Mongo collection + repository interface/implementation, following the existing `Project`/`User` pattern |
| "Row Level Security (RLS)" | Server-side authorization in the Application/Api layer (`[Authorize]`, role/ownership checks in services) — MongoDB has no RLS, so access control must be enforced in C# before data leaves the API |
| "Edge function" / "secure server-side logic" (e.g. scoring, secret validation) | An `Template.Application` service method exposed through a `Template.Api` controller endpoint — never computed or exposed in the frontend |
| "Supabase Auth" / "email+password login" | The existing JWT auth flow (`AuthController`, `AuthService`, `useAuth.tsx`, `ProtectedRoute.tsx`) |
| "Never expose X to the browser" | X must not appear in any DTO/response sent to the client for that role — verify by reading the actual controller response, not by assumption |
| "Admin dashboard" / "protected pages" | Routes nested under the existing `<ProtectedRoute />` element in `App.tsx`, gated further by role checks where the spec distinguishes roles |
| "CSV export", "charts", "tables" | Plain frontend features using existing deps (`recharts` for charts, no new charting lib) — implement client-side export from already-fetched data unless the dataset is too large for the browser |

If a brief requires something this stack genuinely cannot express well (e.g.
true real-time subscriptions, vector search), say so explicitly and propose
the closest reasonable equivalent instead of silently reaching for a new
BaaS/service.

---

## 2. How to read a new project brief

When the user pastes a brief describing a new app:

1. **Identify the shape**: users/roles, core entities, screens/flows,
   security-sensitive data (correct answers, prices, PII, secrets — anything
   that must stay server-side), and explicit acceptance criteria.
2. **Extract the project name** from the brief (usually the PROJECT title or
   the explicit product name the user gives). That name — not `VoiceFlow`,
   `VoiceFlowStudio`, `Template`, or any other placeholder left by a prior
   build — is the single source of truth for naming (see **Project naming**
   in §3).
3. **Confirm Project Controller Mode** before cleanup or build. Read
   `VITE_PROJECT_CONTROLLER_MODE` in `.env` (and `src/config/appMode.ts`).
   This flag controls whether the app is **multi-project** (`true`: post-login
   `/projects`, `ProjectContext`, projects API, project-scoped routes) or
   **single-app** (`false`: straight to the app's main routes, no project
   picker). If the brief does not clearly require multi-tenant / multi-project
   switching, **ask the user whether it should be `true` or `false`** — do
   not guess and do not silently flip the env var. Only after they answer,
   set `.env` accordingly and wire routing (keep `/projects` + `ProjectContext`
   when `true`; use the single-app path and leave controller scaffolding
   unused when `false`).
4. **Map it onto this stack** using §1's table. Note anywhere the brief
   assumes a different backend/infra and translate it silently rather than
   asking the user to restate it in .NET terms.
5. If the brief is ambiguous on something else you can't safely default (e.g.
   unclear whether an action needs auth, unclear data-retention rules),
   ask — don't guess on anything security- or data-model-relevant.
6. Otherwise, proceed straight to cleanup (§3) then build (§4). Don't wait
   for a second confirmation once the brief is clear — pasting the brief
   *is* the build instruction. **Exception:** Project Controller Mode (step 3)
   always requires an explicit user answer when the brief is silent on it.

---

## 3. Cleanup: template is not a museum

Every prior project leaves artifacts behind in this repo (leftover pages,
routes, integrations, migrations, docs, env vars, namespaces). Before
building a new project from a new brief:

- **Remove** anything that is specific to the *previous* project's domain:
  feature pages that only make sense for that old app, its API routes/
  controllers/services/entities, its docs (e.g. root `README.md` content
  describing that old feature), its env vars, its DB collections/migrations,
  third-party integrations it pulled in that the new brief doesn't need
  (e.g. HubSpot OAuth, SIP/WebRTC, `src/integrations/supabase`).
- **Keep** the generic scaffolding: build tooling (Vite/Tailwind/ESLint/
  TS configs), shadcn/ui component library in `src/components/ui`, the
  Clean Architecture layering and its plumbing (JWT auth, Mongo client
  factory, error-handling middleware, `Result<T>`, DI registration,
  localization scaffold, Docker/CI setup), `AuthProvider`/`ProtectedRoute`,
  `AppShell` layout shell, i18n setup, testing setup (`vitest`, xunit
  test projects).
- **The chrome around every project is fixed — do not change the layout shell.**
  `AppShell` (`src/layouts/AppShell.tsx`) and everything it renders — header/top
  bar (`AppTopBar`/`FancyTopBar`), sidebar (`AppSidebar`/`FancySidebar`),
  layout-template switching (`src/layouts/template/*`, theme/palette/style hooks),
  the CopilotKit integration (`CopilotProvider`/`CopilotAppContext`), i18n/RTL
  handling, and focus mode — stay **exactly** as they are. A new project's
  brief is implemented as **pages rendered through `<Outlet />` inside this
  shell** (new routes in `src/App.tsx`, new files in `src/pages` or
  `src/features/*`), plus **new entries in the existing sidebar/top-nav link
  lists** pointing at those routes.

  **Forbidden (layout regressions):**
  - Editing, restyling, or restructuring `AppShell`, `AppTopBar`, `AppSidebar`,
    `FancyTopBar`, `FancySidebar`, or their parent/child DOM hierarchy.
  - Adding duplicate chrome the shell already provides — e.g. rendering `<Logo />`
    (or another brand header) inside a page *and* leaving the shell logos in
    place, which stacks two logos vertically; moving the logo to a page when it
    already lives in the top bar and sidebar; extra sticky headers, second
    sidebars, or wrapper layouts around `<Outlet />`.
  - Changing shell spacing, collapse behavior, RTL flipping, or template
    switching logic to "fit" a new project.
  - Replacing, gutting, or re-theming the shell to match project branding.

  **Allowed without touching shell files:** new route targets, new nav labels/
  icons in the existing `workspace` / `manage` (or equivalent) arrays inside
  `AppSidebar` / `AppTopNav`, and all content *inside* page components only.

  Before finishing any UI work, visually sanity-check authenticated pages: one
  logo per shell region as designed, no stacked duplicates, no broken sidebar/
  header alignment. If a brief explicitly asks for different chrome/branding,
  flag that as a deviation and confirm before touching `AppShell` or its
  children.
- **Don't ask permission for routine cleanup** implied by starting a new
  project on this template — deleting an old feature page or an unused
  integration when replacing it with the new brief's equivalent is expected,
  not a destructive surprise. Do still confirm before deleting anything that
  looks like it might be the user's separate, uncommitted in-progress work
  rather than template cruft.
- **Project Controller Mode — always confirm with the user.** The env flag
  `VITE_PROJECT_CONTROLLER_MODE` (`src/config/appMode.ts`,
  `PROJECT_CONTROLLER_MODE`) is not a safe default. On every new brief:
  check the current value, explain briefly what `true` vs `false` means (see
  §2 step 3), and **ask the user** if the brief does not already decide it.
  Then align `.env`, post-login routes, and whether `/projects` +
  `ProjectContext` / projects API are in the active path. Do not enable
  multi-project controller UI for a single-app product without confirmation;
  do not strip controller scaffolding while the flag is still `true` or the
  user asked for multi-project mode.
- **Project naming — never VoiceFlow.** This repo may still contain
  `VoiceFlowStudio` namespaces, `voiceflow_studio` Mongo DB names,
  `voiceflow-studio` JWT issuer/audience values, `vf_*` localStorage keys,
  and similar strings from an old app built on the template. **None of that
  is the current project's identity.** When a new brief arrives, derive a
  project slug from the prompt (e.g. "Graduate Assessment" →
  `GraduateAssessment` / `graduate_assessment` / `graduate-assessment` for
  different contexts) and use **only that name** — or an obvious variant of
  it — everywhere you name or rename things during cleanup and build:
  - **Backend**: C# namespaces, assembly/project folder names if you rename
    them, Swagger/API title, JWT `Issuer`/`Audience`, seed defaults, log
    labels.
  - **Frontend**: page/document title, env prefixes, `localStorage` keys,
    TypeScript types/interfaces named after the old product.
  - **Database & deploy**: MongoDB database name in `appsettings.json` and
    `docker-compose.yml`, Docker image/env defaults, README project title.
  Rename away any `VoiceFlow*` / `voiceflow_*` / `vf_*` artifact you touch
  while implementing the new brief. Do not ship a new project still branded
  VoiceFlow. A full repo-wide rename of untouched template files is not
  required as a standalone task — but anything in scope for the new build
  must use the prompt's project name, not the template's history.

---

## 4. Building the new project

- **Entities/data model**: add to `Template.Core/Entities`, repository
  interface in `Template.Core/Interfaces`, Mongo implementation + indexes in
  `Template.Infrastructure/Repositories` + `Persistence/Indexes`, DTOs in
  `Template.Contracts`, AutoMapper profile in `Infrastructure/Mapping`,
  business logic in a new `Template.Application` service, controller in
  `Template.Api/Controllers`. Follow the existing `Project` entity/service/
  controller as the reference pattern for a new resource.
- **Server-authoritative logic**: anything the spec says must not be
  trusted to the client (scoring, pricing, correct answers, permission
  checks) lives entirely in `Template.Application` services and is invoked
  through an API endpoint — the frontend only ever sends raw input and
  receives a result, never the logic or the answer key.
- **Frontend pages/routes**: new pages in `src/pages`, wired into
  `src/App.tsx` inside or outside `<ProtectedRoute />` depending on whether
  they require auth, using `AppShell` for authenticated layout. Fetch data
  via `react-query` hooks calling `apiClient`, not direct `fetch`/axios
  scattered around components.
- **Styling**: use Tailwind + existing shadcn/ui primitives; only add a new
  UI dependency if the brief needs something shadcn genuinely can't do
  (e.g. a specific chart type `recharts` doesn't support).
- **Tests**: extend the existing xunit test projects under `backend/tests`
  for new Application services, and `vitest`/RTL under `src/test` for
  frontend logic that has real branching (scoring, band mapping, timers) —
  don't skip tests for security-relevant logic like server-side scoring.
- **Config**: new tunables (time limits, feature toggles) go through a
  `settings`-style entity/table the admin can edit, not hardcoded constants,
  whenever the brief says "admin can configure X without touching code."
- **Seed/reference data always goes through the app, never hand-written into
  Mongo.** Entities in `Template.Core/Entities` mark their `Id` (and any
  foreign-key field referencing another entity's `Id`, e.g. `SectionId`) with
  `[BsonRepresentation(BsonType.ObjectId)]` — the C# driver only converts a
  `string` to/from a real BSON `ObjectId` when the write goes through that
  entity's serializer (i.e. `repository.InsertAsync(...)` or equivalent app
  code). If reference/seed data is instead inserted by hand — pasting JSON
  into Studio 3T/Compass, a raw `mongoimport`, a manually-written shell
  script — any foreign-key field will land as a plain BSON `String` instead
  of `ObjectId`. Queries that filter on that field via a strongly-typed LINQ
  expression (`_col.Find(x => x.SectionId == id)`) then silently match zero
  documents, because MongoDB treats `ObjectId("...")` and `"..."` as
  different types even when the hex value matches — while any query that
  doesn't touch that field (e.g. a filter on an unrelated bool) still
  returns full results. That split (one endpoint empty, a sibling endpoint
  fine) is the signature of this bug — check actual BSON types in
  Studio 3T/Compass (`ObjectId(...)` vs a plain quoted string) before
  assuming it's a code or auth problem. **Always seed through a seed
  endpoint/service that calls the repository's `Insert`, or a script that
  goes through the C# driver — never write reference documents directly
  into MongoDB.**

---

## 5. Non-negotiables

- Never send secret/authoritative data (correct answers, other users'
  private data, scoring weights meant to stay internal) to the browser,
  including via network tab inspection — verify the actual DTO, not the
  intent.
- Never invent a second backend/BaaS alongside the .NET+Mongo one for a new
  project's core data. One backend per app.
- Don't leave a half-migrated repo: if you strip a previous project's
  feature, remove its dead imports/routes/env references too, don't just
  delete the page component.
- Never leave a new project running under VoiceFlow / `voiceflow_*` names
  in config, code, or UI that you added or modified for that build — use the
  project name from the brief (§3, **Project naming**).
- Never modify the app shell layout (`AppShell` and its header/sidebar
  children) or introduce layout defects such as duplicate logos, double
  headers, or extra wrappers around page content (§3, **The chrome around
  every project is fixed**).
- Never assume `VITE_PROJECT_CONTROLLER_MODE` — read it, and if the brief
  is silent, ask the user `true` or `false` before routing/cleanup (§2 step 3,
  §3 **Project Controller Mode**).
- Follow this repo's existing "Executing actions with care" conventions from
  the harness system prompt for anything destructive beyond routine template
  cleanup (force-push, dropping prod data, etc.) — ask first.
