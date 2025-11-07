# Mini Job Board

Three-layer ASP.NET Core MVC app with EF Core (Code-First) and Identity.

- .NET: 8.0
- Layers: Web (MVC), Application (interfaces/DTOs), Infrastructure (DbContext, Entities, EF configs, service impl)
- Persistence: SQLite by default; EF InMemory toggle for demos/tests
- Identity: custom password policy (≥2 uppercase, ≥3 digits, ≥3 symbols)

## Run (dev)
1) From repo root: `dotnet restore`
2) Update DB (SQLite): `dotnet tool restore` (if needed) then `dotnet ef database update --project MiniJobBoard.Infrastructure --startup-project MiniJobBoard.Web`
3) Run: `dotnet run --project MiniJobBoard.Web`

To use InMemory provider for demo, set in `MiniJobBoard.Web/appsettings.Development.json`:
```json
{
  "Persistence": { "UseInMemory": true }
}
```

## Project structure
- MiniJobBoard.sln
- MiniJobBoard.Web (MVC + Identity)
- MiniJobBoard.Application (interfaces, DTOs)
- MiniJobBoard.Infrastructure (DbContext, Entities, EF, services)

## Next tasks
- Add DbContext, entities, and EF configurations (Infrastructure)
- Wire Identity to use Infrastructure DbContext
- Implement password validator and DI
- Define service interfaces (Application) and implementations (Infrastructure)
- Controllers/Views for Jobs, Applications, Employers

See PROJECT_PLAN.md for detailed phases and commit grouping guidance.
