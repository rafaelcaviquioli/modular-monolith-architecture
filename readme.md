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

- **Single deployable application**
- **Multiple independent modules**
- Each module owns:
  - Domain
  - Application logic
  - Infrastructure
  - Database schema (separate EF Core DbContext)
- Modules communicate via:
  - **Contracts** (synchronous, via `IMessageBus.InvokeAsync`)
  - **Integration Events** (asynchronous, via `IMessageBus.PublishAsync`)

---

# Solution Structure

```
ModularMonolith.sln

Monolith.Bootstrapper/          ← Single executable; wires all modules + Wolverine

src/
├── BuildingBlocks/
│   ├── Monolith.BuildingBlocks.Domain          ← Entity, AggregateRoot, ValueObject, IDomainEvent
│   ├── Monolith.BuildingBlocks.Application     ← ICommand, IQuery marker interfaces
│   └── Monolith.BuildingBlocks.Infrastructure  ← IDbConnectionFactory
│
└── Modules/
    ├── Orders/
    │   ├── Monolith.Modules.Orders             ← internal: Domain, Application, Infrastructure, API
    │   └── Monolith.Modules.Orders.Contracts   ← public: DTOs, Requests, IOrdersModule, integration events
    │
    └── Users/
        ├── Monolith.Modules.Users              ← internal: Domain, Application, Infrastructure, API
        └── Monolith.Modules.Users.Contracts    ← public: DTOs, Requests, IUsersModule, integration events

tests/
├── UnitTests/
│   ├── Monolith.Modules.Orders.Tests
│   └── Monolith.Modules.Users.Tests
│
└── IntegrationTests/
    └── Monolith.IntegrationTests
```

---

# Modules

Each module represents a **business capability** and is **independent** from other modules.

```
Monolith.Modules.Orders
│
├── API
│   └── OrdersController.cs              ← internal; uses IMessageBus
│
├── Application
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
├── Domain
│   ├── Entities/Order.cs
│   ├── Enums/OrderStatus.cs
│   └── Events/OrderPlacedDomainEvent.cs
│
├── Infrastructure
│   └── Persistence/
│       ├── OrdersDbContext.cs
│       └── Configurations/OrderConfiguration.cs
│
├── AssemblyInfo.cs                      ← [assembly: WolverineModule]
└── OrdersModule.cs                      ← public; AddOrdersModule() extension
```

Each module contains **everything needed to implement the business logic of that domain**.

---

# Module Contracts

Modules do **not** reference another module's internal projects.

Instead, they use **Contracts** — a standalone project with no dependencies on the module's internals.

```
Monolith.Modules.Orders.Contracts
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

Other modules depend only on `Orders.Contracts`, never on `Orders`, `Orders.Domain`, or `Orders.Application`.

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
- `[assembly: WolverineModule]` on each module assembly

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
Monolith.Modules.Orders/Infrastructure/
  └── Persistence/
      ├── OrdersDbContext.cs
      └── Configurations/OrderConfiguration.cs
```

There is **no shared infrastructure layer for modules**.

---

# Tests

```
tests/
├── UnitTests/
│   ├── Monolith.Modules.Orders.Tests   ← domain logic
│   └── Monolith.Modules.Users.Tests    ← domain logic
└── IntegrationTests/
    └── Monolith.IntegrationTests       ← API + cross-module scenarios
```

**Unit tests** cover domain entities and aggregate behaviour (pure logic, no EF/Wolverine).
**Integration tests** cover API endpoints, EF Core persistence, and end-to-end flows.

Run all tests:

```bash
dotnet test ModularMonolith.sln
```

---

# Internal Module Boundaries

All implementation types inside a module are `internal`. Only Contracts types and the module entry point (`XxxModule.cs`) are `public`.

```csharp
internal class Order : AggregateRoot<Guid> { }
internal class PlaceOrderCommandHandler { }
internal class OrdersDbContext : DbContext { }
```

---

# InternalsVisibleTo

Unit tests need access to internal types.
Declared in each module's `AssemblyInfo.cs`:

```csharp
[assembly: WolverineModule]
[assembly: InternalsVisibleTo("Monolith.Modules.Orders.Tests")]
```

Only the module's **own** test project is granted visibility.

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
