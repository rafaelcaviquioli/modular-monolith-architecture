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


src/
├── Monolith.Bootstrapper/                      ← Single executable; wires all modules + Wolverine
│
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

Example module:

```
Monolith.Modules.Orders
│
├── API
│   └── OrdersController.cs
│
├── Application
│   ├── Commands
│   ├── Queries
│   └── EventHandlers
│
├── Domain
│   ├── Entities
│   ├── ValueObjects
│   └── DomainEvents
│
├── Infrastructure
│   ├── Persistence
│   │   ├── OrdersDbContext.cs
│   │   └── Repositories
│   ├── Configurations
│   └── Migrations
│
└── OrdersModule.cs
```

Each module contains **everything needed to implement the business logic of that domain**.

---

# Module Contracts

Modules do **not communicate with each other's internal implementation**.

Instead, they use **Contracts**.

Example:

```
Monolith.Modules.Orders.Contracts
│
├── Dtos
├── Requests
├── Services
└── Events
```

Example service contract:

```csharp
public interface IOrdersModule
{
    Task<Guid> PlaceOrder(CreateOrderRequest request);
    Task<OrderDto> GetOrder(Guid orderId);
}
```

Other modules depend only on:

```
Orders.Contracts
```

Never on:

```
Orders.Domain
Orders.Application
Orders.Infrastructure
```

---

# Database Strategy

Each module owns its **own EF Core DbContext**.

Example:

```
OrdersDbContext
PaymentsDbContext
UsersDbContext
```

Benefits:

* Prevents cross-module queries
* Strong ownership of data
* Independent migrations
* Clear transaction boundaries

Recommended database layout:

```
orders.*
payments.*
users.*
```

Each module controls its own schema.

---

# Module Communication

Modules communicate in two ways.

## 1. Synchronous (Contracts)

Example:

```
Payments → IOrdersModule
```

Used for direct queries or commands.

---

## 2. Asynchronous (Integration Events)

Events are defined in the **Contracts project**.

Example:

```
OrderPlacedIntegrationEvent
```

Flow:

```
Orders Module
     ↓
Publish Event
     ↓
Payments Module Handles Event
```

This enables **loose coupling** between modules.

---

# Infrastructure Placement

Infrastructure always belongs to the **module that owns the domain**.

Example:

```
Orders.Infrastructure
```

Contains:

* EF Core DbContext
* Repositories
* External integrations
* Persistence configurations
* Migrations

There is **no shared infrastructure layer for modules**.

---

# Tests

Tests are organized per module.

```
tests
│
├── Monolith.Modules.Orders.Tests
├── Monolith.Modules.Payments.Tests
└── Monolith.Modules.Users.Tests
```

Types of tests:

### Unit Tests

Test:

* Domain logic
* Application use cases

### Integration Tests

Test:

* API endpoints
* Database integration
* Cross-module communication

---

# Internal Module Boundaries

Modules enforce strict boundaries using:

* `internal` visibility
* `InternalsVisibleTo`
* architecture analyzers

Most classes inside modules are **internal**.

Example:

```csharp
internal class Order
```

This prevents other modules from accessing internal implementation.

---

# InternalsVisibleTo

Unit tests need access to internal types.

This is enabled using:

```csharp
[assembly: InternalsVisibleTo("Monolith.Modules.Orders.Tests")]
```

This allows:

```
Orders.Tests → access Orders internals
```

But prevents:

```
Payments module → accessing Orders internals
```

---

# Architecture Rules

Architecture rules are enforced using analyzers such as:

* NetArchTest
* ArchUnitNET

These rules run as part of the test suite.

Example rules:

### Modules cannot depend on other modules

```
Payments → Orders.Domain ❌
Payments → Orders.Application ❌
Payments → Orders.Contracts ✅
```

---

### Domain cannot depend on Infrastructure

```
Domain → Infrastructure ❌
```

---

### Domain cannot depend on EF Core

```
Domain → Microsoft.EntityFrameworkCore ❌
```

---

### Application cannot depend on API

```
Application → API ❌
```

---

# Dependency Direction

Allowed dependency flow:

```
API
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
Application
```

Domain must remain **pure business logic**.

---

# Why This Architecture

This architecture provides:

* Strong domain boundaries
* Independent modules
* Controlled dependencies
* Microservice migration path
* Safer refactoring
* Better scalability of the codebase

If necessary, a module can later be extracted into a **microservice** with minimal changes.

---

# Key Principles

1. **Modules own their domain and data**
2. **Modules communicate via contracts or events**
3. **Internal implementation is hidden**
4. **Architecture rules are enforced automatically**
5. **Infrastructure stays inside modules**

Following these principles keeps the modular monolith **maintainable, scalable, and safe for long-term evolution**.
