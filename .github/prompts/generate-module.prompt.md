---
description: "Scaffold a new module with Contracts project, internal implementation (Domain, Application, Infrastructure, API), test project, and wiring into the Bootstrapper."
agent: "modular-monolith"
argument-hint: "Module name (e.g., Payments)"
tools: [read, edit, search, execute]
---

Generate a complete module skeleton for the given module name. Follow the project's established patterns exactly.

## What to Create

### 1. Contracts Project — `src/Modules/{Name}/Monolith.Modules.{Name}.Contracts/`
- `.csproj` (net10.0, no project references)
- `Dtos/{Name}Dto.cs` — public record with basic properties
- `Requests/Create{Name}Request.cs` — public record
- `Services/I{Name}Module.cs` — public interface with create and get methods
- `IntegrationEvents/` — one example integration event as a plain record

### 2. Module Project — `src/Modules/{Name}/Monolith.Modules.{Name}/`
- `.csproj` referencing Contracts, BuildingBlocks.Domain, BuildingBlocks.Application, BuildingBlocks.Infrastructure
- NuGet: WolverineFx, WolverineFx.EntityFrameworkCore, Microsoft.EntityFrameworkCore.InMemory
- `Domain/Entities/` — one aggregate root (internal)
- `Domain/DomainEvents/` — one domain event (internal)
- `Application/Commands/` — one command + handler (internal)
- `Application/Queries/` — one query + handler (internal)
- `Application/DomainEventHandlers/` — bridges domain event to integration event
- `Infrastructure/Persistence/{Name}DbContext.cs` — internal, own schema
- `Infrastructure/Persistence/Configurations/` — entity type configuration
- `API/{Name}Controller.cs` — internal, uses IMessageBus
- `{Name}Module.cs` — public static, Add/Use extension methods
- `{Name}ModuleService.cs` — internal, implements I{Name}Module
- `AssemblyInfo.cs` — InternalsVisibleTo + WolverineModule

### 3. Test Project — `tests/UnitTests/Monolith.Modules.{Name}.Tests/`
- xunit project referencing the module
- One domain entity test

### 4. Wire Up
- Add project reference in Bootstrapper `.csproj`
- Register module in `Program.cs`
- Add all projects to `ModularMonolith.sln` with correct solution folders
- Update `readme.md` to include the new module

### 5. Verify
- Run `dotnet build ModularMonolith.sln`
- Run `dotnet test`
