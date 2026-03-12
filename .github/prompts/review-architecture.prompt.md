---
description: "Review the solution for modular monolith architecture rule violations: dependency direction, module boundaries, visibility, and documentation drift."
agent: "modular-monolith"
tools: [read, search]
---

Review the entire solution for architecture rule violations. Check every module systematically.

## Rules to Verify

### Module Boundaries
- No module references another module's internal project (only `.Contracts` allowed)
- Integration events live in Contracts, domain events stay internal
- No cross-module DbContext queries

### Dependency Direction
- Domain layer: ZERO infrastructure dependencies (no EF Core, no Wolverine, no ASP.NET references)
- Application layer does not depend on API layer
- Flow: `API → Application → Domain`, `Infrastructure → Application`

### Visibility
- Only Contracts types and module registration entry points are `public`

### Wolverine
- No MediatR references anywhere
- `[assembly: WolverineModule]` present on every module assembly
- Handlers follow naming convention: class suffix `Handler`, method `Handle`/`HandleAsync`

### Documentation
- `readme.md` matches the current solution structure and patterns
- No outdated sections describing things that no longer exist

## Output Format

For each violation found, report:
- **File**: path and line
- **Rule**: which rule is broken
- **Fix**: what to change

If no violations found, confirm compliance.
