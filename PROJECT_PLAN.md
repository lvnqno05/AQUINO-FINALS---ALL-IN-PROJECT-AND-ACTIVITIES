# Mini Job Board — Project Plan

Status: Draft. Items marked [CONFIRM] need your confirmation.

## Goals
- EF Core Code-First with DbContext and InMemory provider for demo/tests.
- ASP.NET Core MVC with layered architecture (Presentation, Application, Infrastructure).
- Use interfaces for services; DI throughout.
- ASP.NET Core Identity with custom password policy: ≥2 uppercase, ≥3 digits, ≥3 symbols.
- GitHub repo + GitHub Projects; small commits (≤5 files per commit); no ZIP/bulk uploads.
- Ready for live debugging.

## Decisions and Assumptions
- .NET: .NET 8 [CONFIRM].
- EF Core: 8.x.
- Persistent DB: SQLite (file: `app.db`) for simplicity; switchable to SQL Server later.
- Demo/Tests DB: EF Core InMemory provider via config switch.
- Roles: Admin, Employer, Applicant (you answered “yes” to Admin).
- Resume: File upload stored under `wwwroot/uploads/resumes` (max 5 MB) [CONFIRM size].
- Repo name: [CONFIRM, e.g., `mini-job-board`].

## Solution Structure (3-layer)
- Presentation: `MiniJobBoard.Web`
  - ASP.NET Core MVC + Identity UI, Controllers, Razor Views, static files.
- Application: `MiniJobBoard.Application`
  - Interfaces, DTOs/ViewModels, validation contracts.
- Infrastructure: `MiniJobBoard.Infrastructure`
  - EF Core DbContext, Entities, Fluent configurations, Migrations, Service implementations, File storage.

Project references:
- Web → Application, Infrastructure
- Infrastructure → Application
- Application → (no project refs)

## Domain Model (initial)
- ApplicationUser (extends IdentityUser)
  - FirstName, LastName, CreatedAt
- EmployerProfile
  - Id (PK), UserId (FK → ApplicationUser), CompanyName, Description, Website, CreatedAt
- Job
  - Id, EmployerId (FK → EmployerProfile), Title, Description, Location, SalaryMin, SalaryMax, IsActive, CreatedAt
- JobApplication
  - Id, JobId (FK → Job), ApplicantUserId (FK → ApplicationUser), CoverLetter, ResumePath, Status (Pending/Accepted/Rejected), AppliedAt

Relationships:
- ApplicationUser 1—0..1 EmployerProfile
- EmployerProfile 1—* Job
- Job 1—* JobApplication
- ApplicationUser 1—* JobApplication (as Applicant)

## EF Core
- DbContext: `AppDbContext`
  - DbSets: Users (via Identity), EmployerProfiles, Jobs, JobApplications
  - Fluent API for keys, relationships, and constraints
- Migrations: Code-First; initial migration creates Identity + domain tables
- Seeding:
  - Roles: Admin, Employer, Applicant
  - Admin account (email user+pass provided at runtime via secrets/ENV)
  - Optional demo data when InMemory is enabled

## Identity & Security
- Custom `IPasswordValidator<ApplicationUser>` enforcing:
  - ≥2 uppercase A–Z
  - ≥3 digits 0–9
  - ≥3 symbols (punctuation)
  - Length ≥8 (alongside standard options)
- Role-based authorization policies:
  - Employer-only: manage jobs
  - Applicant-only: apply to jobs
  - Admin: role/seed oversight

## Configuration
- `appsettings.json` (SQLite default)
  - ConnectionStrings:DefaultConnection → `Data Source=app.db`
  - Persistence:UseInMemory → false
  - FileUpload:MaxResumeSizeBytes → 5_242_880 (≈5 MB)
- `appsettings.Development.json` can set `UseInMemory=true` for quick demo

Example keys:
```
ConnectionStrings:DefaultConnection = "Data Source=app.db"
Persistence:UseInMemory = false
FileUpload:MaxResumeSizeBytes = 5242880
```

## Services (interfaces in Application; impl in Infrastructure)
- IJobService
  - CreateJob, UpdateJob, ArchiveJob, GetJob(id), ListActiveJobs, Search(query)
- IEmployerService
  - GetEmployerProfile(userId), UpsertEmployerProfile, ListEmployerJobs
- IApplicationService
  - Apply(jobId, applicantUserId, coverLetter, resumeFile), ListApplicationsForJob(jobId), ListMyApplications(userId), UpdateStatus
- IFileStorage
  - SaveResume(IFormFile file) → ResumePath, Delete(path)

## MVC (initial endpoints)
- JobsController
  - GET /Jobs, GET /Jobs/{id}
  - Employer-only: GET/POST /Jobs/Create, GET/POST /Jobs/Edit/{id}, POST /Jobs/Archive/{id}
- ApplicationsController
  - Applicant-only: GET /Applications/Mine, POST /Applications/Apply/{jobId}
  - Employer-only: GET /Applications/ForJob/{jobId}, POST /Applications/{id}/Status
- EmployersController
  - Employer dashboard/profile

Razor Views
- List/Details for Jobs, Create/Edit, Apply form, My Applications, Employer dashboard

## Testing & InMemory
- Configure test host to use InMemory provider
- Minimal unit tests for password validator and services
- Seed demo data when `UseInMemory=true`

## GitHub Workflow
- Repository: [CONFIRM name]
- GitHub Project board columns: Backlog → In Progress → Review → Done
- Issues for each task; link PRs/commits
- Commits: ≤5 files changed per commit; meaningful messages
- Branching: `main` default; optional feature branches per phase

## Phase Plan (task decomposition)

Phase 0 — Repo setup
- Create repo, .gitignore (VisualStudio, .NET), README skeleton
- Create GitHub Project and columns; add initial issues/cards

Phase 1 — Solution scaffolding
- Create solution and 3 projects (Web, Application, Infrastructure)
- Wire references; add DI placeholders

Phase 2 — Identity baseline
- Add ASP.NET Core Identity to Web; scaffold Identity UI if needed
- Create ApplicationUser; register in DI

Phase 3 — Password policy
- Implement custom IPasswordValidator and register
- Add unit test for validator

Phase 4 — Domain & EF Core
- Add entities, DbContext, Fluent configs
- Add initial migration (SQLite); update database

Phase 5 — InMemory toggle & seeding
- Add config switch and seeding service
- Demo data when InMemory=true

Phase 6 — Services
- Define interfaces (Application)
- Implement EF-based services (Infrastructure)

Phase 7 — MVC controllers & views
- Jobs, Applications, Employers (minimal views)
- Authorization policies & filters

Phase 8 — File uploads
- IFileStorage and implementation to `wwwroot/uploads/resumes`
- Validation for size and type; link to JobApplication.ResumePath

Phase 9 — Polish & tests
- Add basic tests for services using InMemory
- README: run steps, demo notes, troubleshooting

Phase 10 — Presentation prep
- Live debugging scenarios (e.g., tweak password validator; fix a failing test)
- Create sample users and flows

## Commit Grouping Guidance (≤5 files/change)
- Group by feature: e.g., “Add AppUser + password validator” touching: ApplicationUser, validator, DI registration, test, csproj (5 files)
- For migrations, isolate schema changes in their own commit when possible

## How to Run (anticipated)
- dev (SQLite):
  - `dotnet ef database update` (from Web project)
  - `dotnet run` (Web)
- demo (InMemory): set `Persistence:UseInMemory=true` then `dotnet run`

## Open Items
- [CONFIRM] .NET 8 target
- [CONFIRM] Repo name
- [CONFIRM] Resume max size (default 5 MB) and allowed types (pdf, docx)
- [OPTIONAL] SQL Server support later

## Presentation Checklist
- Show Identity login/registration and password rules
- Employer: create job; Applicant: browse/apply; Employer: review applications
- Toggle InMemory for demo and show seeded data
- Walk through layered structure and interfaces
