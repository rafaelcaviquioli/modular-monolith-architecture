# Modular Monolith Architecture (.NET)

This repository implements a **Modular Monolith** architecture using .NET 10.
The goal is to achieve **strong modular boundaries**, **clear domain separation**, and **microservice-ready modules** while still deploying a **single application**.

The architecture combines:

- Domain-Driven Design (DDD)
- CQRS via Wolverine
- Domain Events / Integration Events via Wolverine
- Multiple EF Core DbContexts (one per module)
- Explicit module contracts
- Enforced architectural boundaries

---

# High-Level Architecture

- **Single deployable application** — one assembly (`Monolith`)
- **Multiple independent modules** within that assembly
- Each module owns:
  - Domain
  - Application logic
  - Infrastructure
  - Database schema (separate EF Core DbContext)
  - Contracts (DTOs, requests, integration events, module boundary interface)
- Modules communicate via:
  - **Contracts** (synchronous, via `IMessageBus.InvokeAsync`)
  - **Integration Events** (asynchronous, via `IMessageBus.PublishAsync`)

---

# Solution Structure

```
ModularMonolith.slnx

src/
└── Monolith/                   ← Single executable; all modules + Wolverine
    │
    ├── BuildingBlocks/
    │   ├── Domain/             ← Entity, AggregateRoot, ValueObject, IDomainEvent
    │   └── Application/        ← ICommand, IQuery marker interfaces
    │
    └── Modules/
        ├── Orders/
        │   ├── Contracts/      ← public: DTOs, Requests, IOrdersModule, integration events
        │   ├── Domain/
        │   ├── Application/
        │   ├── Infrastructure/
        │   ├── API/
        │   ├── OrdersModule.cs
        │   └── OrdersModuleService.cs
        │
        └── Users/
            ├── Contracts/      ← public: DTOs, Requests, IUsersModule, integration events
            ├── Domain/
            ├── Application/
            ├── Infrastructure/
            ├── API/
            ├── UsersModule.cs
            └── UsersModuleService.cs

tests/
└── Monolith.Tests/             ← All tests (unit + integration)
    ├── Modules/
    │   ├── Orders/Domain/
    │   └── Users/Domain/
    └── Integration/
```

---

# Modules

Each module represents a **business capability** and is **independent** from other modules.

```
Monolith/Modules/Orders/
│
├── Contracts/
│   ├── Dtos/OrderDto.cs
│   ├── Requests/CreateOrderRequest.cs
│   ├── Services/IOrdersModule.cs
│   └── IntegrationEvents/OrderPlacedIntegrationEvent.cs
│
├── API/
│   └── OrdersController.cs              ← internal; uses IMessageBus
│
├── Application/
│   ├── Commands/PlaceOrder/
│   │   ├── PlaceOrderCommand.cs
│   │   └── PlaceOrderCommandHandler.cs
│   ├── Queries/GetOrder/
│   │   ├── GetOrderQuery.cs
│   │   └── GetOrderQueryHandler.cs
│   ├── DomainEventHandlers/
│   │   └── OrderPlacedDomainEventHandler.cs   ← publishes integration event
│   └── IntegrationEventHandlers/
│       └── (handlers for events from other modules)
│
├── Domain/
│   ├── Entities/Order.cs
│   ├── Enums/OrderStatus.cs
│   └── DomainEvents/OrderPlacedDomainEvent.cs
│
├── Infrastructure/
│   └── Persistence/
│       ├── OrdersDbContext.cs
│       └── Configurations/OrderConfiguration.cs
│
├── OrdersModule.cs                      ← public; AddOrdersModule() extension
└── OrdersModuleService.cs               ← internal; IOrdersModule implementation
```

Each module contains **everything needed to implement the business logic of that domain**.

---

# Module Contracts

Modules do **not** reference another module's internal types.

Instead, they use **Contracts** — a sub-namespace within the module's folder with no dependencies on the module's internals.

```
Monolith/Modules/Orders/Contracts/
│
├── Dtos/OrderDto.cs
├── Requests/CreateOrderRequest.cs
├── Services/IOrdersModule.cs
└── IntegrationEvents/OrderPlacedIntegrationEvent.cs
```

Integration events are plain records — no marker interfaces required:

```csharp
public record OrderPlacedIntegrationEvent(
    Guid OrderId, string CustomerName, decimal TotalAmount, DateTime OccurredOn);
```

Other modules reference only types from `*.Contracts.*` namespaces, never from a module's internal namespaces like `*.Domain`, `*.Application`, or `*.Infrastructure`.

## Contract Service vs Command (Important)

In this architecture, a **contract service** (for example `IOrdersModule`) is a **module boundary API**.
It is not the same concept as an Application Service from classic layered architecture.

- **Contract service**: public interface in `*.Contracts` used by other modules. It exposes module capabilities in stable DTO/request types.
- **Command / Query**: internal application message handled by Wolverine inside the owning module.

Think of it as:

- Outside the module: call `IOrdersModule` (boundary contract).
- Inside the module: `IOrdersModule` implementation maps the call to `bus.InvokeAsync(...)` with internal commands/queries.

Concrete mapping in this codebase:

| Boundary API (Contracts) | Internal message (Application) |
|---|---|
| `IOrdersModule.PlaceOrderAsync(CreateOrderRequest)` | `PlaceOrderCommand` |
| `IOrdersModule.GetOrderAsync(Guid)` | `GetOrderQuery` |
| `IUsersModule.CreateUserAsync(CreateUserRequest)` | `CreateUserCommand` |
| `IUsersModule.GetUserAsync(Guid)` | `GetUserQuery` |

This separation gives two benefits:

- Modules stay decoupled because callers only reference `*.Contracts`.
- The owning module can evolve internal commands/handlers without leaking internals across boundaries.

### What "service" means here

`IOrdersModule` / `IUsersModule` are best read as **module gateway interfaces**:

- They are intentionally thin.
- They do not contain domain logic.
- They orchestrate dispatch to Wolverine messages.
- They provide a convenient synchronous facade for in-process module-to-module calls.

So, when you see "service" in `Contracts/Services`, read it as **service contract at the module boundary**, not as "application service layer" owning business rules.

---

# Database Strategy

Each module owns its **own EF Core DbContext** with its own schema.

- `OrdersDbContext` — schema `orders`
- `UsersDbContext` — schema `users`

Benefits:
- Prevents cross-module queries
- Strong ownership of data
- Independent migrations
- Clear transaction boundaries

Dev uses `InMemory` provider. Prod uses SQL Server or Postgres — swap the provider in `AddXxxModule()` and add `PersistMessages*` in the Wolverine outbox config.

There is **no shared infrastructure layer** between modules.

---

# Wolverine (Messaging Backbone)

[Wolverine](https://wolverine.netlify.app/) is the single messaging backbone. There is no MediatR.

Handlers are discovered by convention — no registration needed:
- Class suffix: `Handler`
- Method name: `Handle` or `HandleAsync`
- `[assembly: WolverineModule]` in `AssemblyInfo.cs` marks the single assembly for handler discovery

```csharp
// Bootstrapper wiring
builder.Host.UseWolverine(opts =>
{
    opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated; // each module's handlers run independently
    opts.Policies.AutoApplyTransactions();                             // outbox-ready via WolverineFx.EntityFrameworkCore
});
```

| Operation | API |
|---|---|
| Command / Query (wait for result) | `bus.InvokeAsync<TResult>(message)` |
| Fire-and-forget event | `bus.PublishAsync(event)` |

---

# Module Communication

## 1. Synchronous — via Contracts

Preferred shape for cross-module sync calls:

1. Caller references target module `*.Contracts`.
2. Caller invokes `IOrdersModule` / `IUsersModule`.
3. Module service implementation translates to `InvokeAsync` command/query.

`InvokeAsync` can still be used directly, but only with types exposed from Contracts. Do not reference another module's internal command/query types.

## 2. Asynchronous — Integration Events

Domain event handlers publish **integration events** (plain records in `.Contracts`).
Other modules subscribe by writing a `HandleAsync(IntegrationEvent)` handler.

```
Orders Module
   └─ OrderPlacedDomainEventHandler
         └─ publishes OrderPlacedIntegrationEvent
                               ↓
              Users Module
                 └─ OrderPlacedIntegrationEventHandler
```

The consuming module references only `Orders.Contracts` — never the `Orders` internal project.

`MultipleHandlerBehavior.Separated` ensures each module's handler for the same event runs as an independent pipeline.

---

# Infrastructure Placement

Infrastructure belongs to the **module that owns the domain**.

```
Monolith/Modules/Orders/Infrastructure/
  └── Persistence/
      ├── OrdersDbContext.cs
      └── Configurations/OrderConfiguration.cs
```

There is **no shared infrastructure layer for modules**.

---

# Tests

```
tests/
└── Monolith.Tests/
    ├── Architecture/        ← enforced module boundary rules (NetArchTest)
    ├── Modules/
    │   ├── Orders/Domain/   ← order aggregate tests
    │   └── Users/Domain/    ← user aggregate tests
    └── Integration/         ← API + cross-module scenarios
```

**Unit tests** cover domain entities and aggregate behaviour (pure logic, no EF/Wolverine).
**Integration tests** cover API endpoints, EF Core persistence, and end-to-end flows.
**Architecture tests** use [NetArchTest](https://github.com/BenMorris/NetArchTest) to enforce module boundaries at the type level — they run on every build and catch violations early.

## Architecture Rules Tested

| Rule | Description |
|---|---|
| Module isolation | Internal types (`Domain`, `Application`, `Infrastructure`, `API`) must not depend on another module's internals — only `*.Contracts.*` may cross module boundaries |
| Pure domain | Domain types must have zero dependencies on EF Core, Wolverine, or ASP.NET Core |
| Layer flow | `Application` must not depend on `Infrastructure` or `API` within the same module |
| Domain boundary | Domain must not depend on `Application`, `Infrastructure`, or `API` |
| Contracts purity | Contracts must not expose domain types — only DTOs, requests, and integration events |
| Handler convention | Application handler classes must use the `*Handler` naming suffix (Wolverine discovery) |

To add a new module, add its root namespace to `ModuleNamespaces` in `Architecture/ModuleArchitectureTests.cs`.

Run all tests:

```bash
dotnet test ModularMonolith.slnx
```

---

# Internal Module Boundaries

All implementation types inside a module are `internal`. Only the module registration entry point (`XxxModule.cs`) and Contracts types are `public`.

```csharp
internal class Order : AggregateRoot<Guid> { }
internal class PlaceOrderCommandHandler { }
internal class OrdersDbContext : DbContext { }
```

---

# InternalsVisibleTo

The test project needs access to internal types.
Declared once in `AssemblyInfo.cs`:

```csharp
[assembly: WolverineModule]
[assembly: InternalsVisibleTo("Monolith.Tests")]
```

---

# Architecture Rules

### Modules cannot depend on other modules' internals

```
Users → Orders.Domain        ❌
Users → Orders.Application   ❌
Users → Orders.Contracts     ✅
```

### Domain has zero infrastructure dependencies

```
Domain → Microsoft.EntityFrameworkCore  ❌
Domain → WolverineFx                    ❌
```

### Application cannot depend on API

```
Application → API ❌
```

### Dependency direction

```
API → Application → Domain
Infrastructure → Application
```

---

# Key Principles

1. **Modules own their domain and data** — no shared DbContexts across modules
2. **Communicate via contracts or integration events** — never via internal types
3. **Internal by default** — implementation details stay hidden
4. **Wolverine for all messaging** — commands, queries, domain events, integration events; no MediatR
5. **Infrastructure stays inside modules** — no shared infrastructure projects
6. **readme.md is always current** — docs describe what IS, not what could be
