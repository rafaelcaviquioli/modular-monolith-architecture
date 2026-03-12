# Project Guidelines

## Stack

- .NET 10 (net10.0), C# 13
- Wolverine for messaging (commands, queries, domain events, integration events) — **no MediatR**
- EF Core with per-module DbContexts (InMemory for dev, SQL Server/Postgres for prod)
- WolverineFx.EntityFrameworkCore for transactional outbox support

## Architecture

Modular monolith with a **single assembly** (`Monolith`). All modules live in `src/Monolith/Modules/`. Building blocks live in `src/Monolith/BuildingBlocks/`. See [readme.md](../readme.md) for full architecture documentation.

- **Modules are independent**: each owns Domain, Application, Infrastructure, API, and Contracts (public DTOs/events/interfaces) — all within the single assembly
- **Cross-module communication**: only via `Modules/*/Contracts/` namespaces (sync) or Integration Events (async through Wolverine)
- **Dependency direction**: `API → Application → Domain`. `Infrastructure → Application`. Domain is pure — no framework dependencies.
- **Visibility**: module implementations are `internal`. Only `Contracts` types and `XxxModule.cs` entry points are `public`.

## Conventions

- Wolverine handler classes: suffix `Handler`, methods `Handle` / `HandleAsync`
- Commands: `IMessageBus.InvokeAsync()`. Queries: `IMessageBus.InvokeAsync<TResult>()`. Events: `IMessageBus.PublishAsync()`
- `[assembly: WolverineModule]` declared once in `AssemblyInfo.cs` (single assembly)
- Module registration: `services.AddXxxModule(configuration)` extension methods in `XxxModule.cs`
- Integration events are plain records in `Modules/*/Contracts/IntegrationEvents/` — no marker interfaces

## Build and Test

```bash
dotnet build ModularMonolith.slnx
dotnet test
```
