# Project Guidelines

## Stack

- .NET 10 (net10.0), C# 13
- Wolverine for messaging (commands, queries, domain events, integration events) — **no MediatR**
- EF Core with per-module DbContexts (InMemory for dev, SQL Server/Postgres for prod)
- WolverineFx.EntityFrameworkCore for transactional outbox support

## Architecture

Modular monolith. See [readme.md](../readme.md) for full architecture documentation.

- **Modules are independent**: each owns Domain, Application, Infrastructure, API, and its own DbContext/schema
- **Cross-module communication**: only via Contracts projects (sync) or Integration Events (async through Wolverine)
- **Dependency direction**: `API → Application → Domain`. `Infrastructure → Application`. Domain is pure — no framework dependencies.
- **Visibility**: module internals are `internal`. Only Contracts types and module entry points are `public`.

## Conventions

- Wolverine handler classes: suffix `Handler`, methods `Handle` / `HandleAsync`
- Commands: `IMessageBus.InvokeAsync()`. Queries: `IMessageBus.InvokeAsync<TResult>()`. Events: `IMessageBus.PublishAsync()`
- Each module assembly has `[assembly: WolverineModule]` for handler discovery
- Module registration: `services.AddXxxModule(configuration)` / `app.UseXxxModule()` extension methods
- Integration events are plain records in `.Contracts` projects — no marker interfaces
- `InternalsVisibleTo` only for the module's own test project

## Build and Test

```bash
dotnet build ModularMonolith.sln
dotnet test
```
