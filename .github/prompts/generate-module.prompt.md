---
description: "Scaffold a new module inside the single Monolith assembly — Contracts, Domain, Features (vertical slices), DomainEventHandlers, Infrastructure, and wiring."
agent: "modular-monolith"
argument-hint: "Module name (e.g., Payments)"
tools: [read, edit, search, execute]
---

Generate a complete module skeleton for the given module name. This is a **single-assembly** project — do NOT create new `.csproj` files. All code goes into `src/Monolith/` and tests go into `tests/Monolith.Tests/`.

Before generating, read the existing Orders and Users modules to understand current conventions:
- `src/Monolith/Modules/Orders/`
- `src/Monolith/Modules/Users/`

## What to Create

All files live inside **`src/Monolith/Modules/{Name}/`**.

### 1. Contracts — `Modules/{Name}/Contracts/`

Public surface of the module. All types here are `public`. No domain types allowed.

- `Contracts/Dtos/Create{Name}Dto.cs` — input DTO (public record)
- `Contracts/Dtos/Get{Name}Dto.cs` — output DTO (public record)
- `Contracts/IntegrationEvents/{Name}CreatedIntegrationEvent.cs` — plain `public record` with an `OccurredOn` `DateTime` property; no marker interfaces
- `Contracts/Services/I{Name}Module.cs` — `public interface` with `Create{Name}Async(Create{Name}Dto, CancellationToken)` and `Get{Name}Async(Guid, CancellationToken)` returning the DTOs; XML summary doc describing it as a gateway, not an application service

### 2. Domain — `Modules/{Name}/Domain/`

Pure domain — **no framework dependencies**.

- `Domain/Entities/{Name}.cs` — `internal class` extending `AggregateRoot<Guid>`
  - Private parameterless constructor for EF Core
  - Static `Create(...)` factory returning `({Name} entity, {Name}CreatedDomainEvent domainEvent)` tuple
  - Properties with `private set`
  - `Id = Guid.NewGuid()` and `CreatedAt = DateTime.UtcNow` in factory
- `Domain/DomainEvents/{Name}CreatedDomainEvent.cs` — `internal record` implementing `IDomainEvent` from `Monolith.BuildingBlocks.Domain`

### 3. Features — `Modules/{Name}/Features/` (vertical slices)

One folder per feature. All types are `internal`.

**`Features/Create{Name}/`**
- `Create{Name}Command.cs` — `internal record` implementing `ICommand<Guid>` from `Monolith.BuildingBlocks.Application`
- `Create{Name}CommandHandler.cs` — `internal class` with constructor-injected `{Name}DbContext`; `Handle` method returns `(Guid, OutgoingMessages)` where `OutgoingMessages` contains the domain event
- `Create{Name}Controller.cs` — `internal class` extending `ControllerBase`, `[ApiController]`, `[Route("api/{names}")]`, `[HttpPost]`, constructor-injected `IMessageBus`; calls `bus.InvokeAsync<Guid>(new Create{Name}Command(...))`; returns `Ok(new { id })`

**`Features/Get{Name}/`**
- `Get{Name}Query.cs` — `internal record` implementing `IQuery<Get{Name}Dto?>` from `Monolith.BuildingBlocks.Application`
- `Get{Name}QueryHandler.cs` — `internal class` with constructor-injected `{Name}DbContext`; `HandleAsync` returns `Get{Name}Dto?`; uses `AsNoTracking()`
- `Get{Name}Controller.cs` — `internal class`, `[HttpGet("{id:guid}")]`, returns `NotFound()` or `Ok(dto)`

### 4. DomainEventHandlers — `Modules/{Name}/DomainEventHandlers/`

Bridges domain events to integration events. All types are `internal`.

- `Publish{Name}CreatedIntegrationEventHandler.cs` — `internal class`; `Handle({Name}CreatedDomainEvent domainEvent)` method that **returns** the integration event record directly (no `OutgoingMessages`, no `void`); Wolverine routes the returned record as the next message

### 5. Infrastructure — `Modules/{Name}/Infrastructure/Persistence/`

- `{Name}DbContext.cs` — `internal class` extending `DbContext(DbContextOptions<{Name}DbContext>)`; `DbSet<{Name}>` property; `OnModelCreating` sets `modelBuilder.HasDefaultSchema("{names_lowercase}")` and applies configuration
- `{Name}Configuration.cs` — `internal class` implementing `IEntityTypeConfiguration<{Name}>`; `ToTable`, `HasKey`, `Property` constraints

### 6. Module entry point — `Modules/{Name}/`

- `{Name}Module.cs` — `public static class`; `AddXxxModule(this IServiceCollection services, IConfiguration configuration)` that:
  - Reads `configuration.GetConnectionString("DefaultConnection")` (throw if null)
  - Registers `{Name}DbContext` with `options.UseNpgsql(connectionString), optionsLifetime: ServiceLifetime.Singleton`
  - Registers `services.AddScoped<I{Name}Module, {Name}ModuleService>()`
- `{Name}ModuleService.cs` — `internal class` implementing `I{Name}Module`; constructor-injected `IMessageBus`; delegates to Wolverine via `bus.InvokeAsync`

## What to Update

### `src/Monolith/Program.cs`
- Add `using Monolith.Modules.{Name};`
- Add `builder.Services.Add{Name}Module(builder.Configuration);`

### `tests/Monolith.Tests/Integration/MonolithApiFixture.cs`
- Add `using Monolith.Modules.{Name}.Infrastructure.Persistence;`
- Add `await scope.ServiceProvider.GetRequiredService<{Name}DbContext>().Database.EnsureCreatedAsync();` in `InitializeAsync`

### `tests/Monolith.Tests/Architecture/ModuleArchitectureTests.cs`
- Add `"Monolith.Modules.{Name}"` to the `ModuleNamespaces` array

### `readme.md`
- Add the new module to the Modules table

## Tests — `tests/Monolith.Tests/Modules/{Name}/Domain/`

Add domain unit tests to the **existing** `Monolith.Tests` project (no new `.csproj`).

- `{Name}Tests.cs` — xUnit `public class`; one fact per factory method; assert entity properties and that the returned domain event contains correct data; follow the pattern in `tests/Monolith.Tests/Modules/Orders/Domain/OrderTests.cs`

## Key Patterns Reference

| Concern | Pattern |
|---|---|
| **Command (returns result)** | `record Cmd(...) : ICommand<Guid>` → `Handle` returns `(Guid, OutgoingMessages)` |
| **Command (no result)** | `record Cmd(...) : ICommand` → `HandleAsync` returns `Task` |
| **Query** | `record Qry(...) : IQuery<TResult>` → `HandleAsync` returns `TResult` |
| **Domain event → integration event** | `Handle(DomainEvent e)` returns the integration event record directly |
| **Aggregate factory** | `static (Entity, DomainEvent) Create(...)` with `private` EF constructor |
| **DbContext** | Own schema, `optionsLifetime: ServiceLifetime.Singleton` |
| **Controller** | `internal`, `IMessageBus` only — no service injection |
| **ModuleService** | Maps DTOs → Commands/Queries via `IMessageBus.InvokeAsync` |
| **Integration events** | Plain `record` in `Contracts/IntegrationEvents/`, no marker interfaces |

## Verify

```bash
dotnet build ModularMonolith.slnx
dotnet test
```
