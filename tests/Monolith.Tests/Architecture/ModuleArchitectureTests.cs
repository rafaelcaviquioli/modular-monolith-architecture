using System.Reflection;
using Mono.Cecil;
using NetArchTest.Rules;
using Xunit;

namespace Monolith.Tests.Architecture;

/// <summary>
/// Enforces architectural boundaries between modules.
/// Rules are derived from the modular monolith conventions documented in readme.md.
/// </summary>
public class ModuleArchitectureTests
{
    // Resolved via any public type in the Monolith assembly
    private static readonly Assembly MonolithAssembly =
        typeof(Monolith.Modules.Orders.OrdersModule).Assembly;

    // Module root namespaces — add new modules here
    private static readonly string[] ModuleNamespaces =
    [
        "Monolith.Modules.Orders",
        "Monolith.Modules.Users",
    ];

    /// <summary>
    /// A module's internal types (Domain, Application, Infrastructure, API) must not
    /// directly reference another module's internal types. Only *.Contracts.* is allowed
    /// as a cross-module dependency.
    /// </summary>
    [Fact]
    public void Module_InternalTypes_ShouldNot_DependOn_OtherModules_Internals()
    {
        var violations = new List<string>();

        foreach (var sourceModule in ModuleNamespaces)
        {
            // Collect non-Contracts sub-namespaces of this module
            var internalNamespaces = new[]
            {
                $"{sourceModule}.Domain",
                $"{sourceModule}.Application",
                $"{sourceModule}.Infrastructure",
                $"{sourceModule}.API",
            };

            foreach (var targetModule in ModuleNamespaces)
            {
                if (targetModule == sourceModule)
                    continue;

                // The only allowed cross-module reference is *.Contracts.*
                var forbiddenNamespaces = new[]
                {
                    $"{targetModule}.Domain",
                    $"{targetModule}.Application",
                    $"{targetModule}.Infrastructure",
                    $"{targetModule}.API",
                };

                foreach (var internalNs in internalNamespaces)
                {
                    foreach (var forbidden in forbiddenNamespaces)
                    {
                        var result = Types
                            .InAssembly(MonolithAssembly)
                            .That()
                            .ResideInNamespaceStartingWith(internalNs)
                            .ShouldNot()
                            .HaveDependencyOnAny(forbidden)
                            .GetResult();

                        if (!result.IsSuccessful)
                            violations.Add(
                                $"[{internalNs}] → [{forbidden}]: "
                                    + string.Join(", ", result.FailingTypeNames ?? [])
                            );
                    }
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Module boundary violations found:\n" + string.Join("\n", violations)
        );
    }

    /// <summary>
    /// Domain types must have zero infrastructure-framework dependencies.
    /// They must not reference EF Core, Wolverine, or ASP.NET Core.
    /// </summary>
    [Fact]
    public void Domain_ShouldNot_HaveFrameworkDependencies()
    {
        var forbiddenDependencies = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Wolverine",
            "Microsoft.AspNetCore",
        };

        foreach (var module in ModuleNamespaces)
        {
            var result = Types
                .InAssembly(MonolithAssembly)
                .That()
                .ResideInNamespaceStartingWith($"{module}.Domain")
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenDependencies)
                .GetResult();

            Assert.True(
                result.IsSuccessful,
                $"[{module}.Domain] has forbidden framework dependencies: "
                    + string.Join(", ", result.FailingTypeNames ?? [])
            );
        }
    }

    /// <summary>
    /// Domain types must not reference Application, Infrastructure, or API layers.
    /// </summary>
    [Fact]
    public void Domain_ShouldNot_DependOn_OuterLayers()
    {
        foreach (var module in ModuleNamespaces)
        {
            var forbiddenDependencies = new[]
            {
                $"{module}.Application",
                $"{module}.Infrastructure",
                $"{module}.API",
            };

            var result = Types
                .InAssembly(MonolithAssembly)
                .That()
                .ResideInNamespaceStartingWith($"{module}.Domain")
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenDependencies)
                .GetResult();

            Assert.True(
                result.IsSuccessful,
                $"[{module}.Domain] depends on outer layers: "
                    + string.Join(", ", result.FailingTypeNames ?? [])
            );
        }
    }

    /// <summary>
    /// Contract types (the public module API surface) must not expose domain types.
    /// Contracts should only contain DTOs, requests, integration events, and interface definitions.
    /// </summary>
    [Fact]
    public void Contracts_ShouldNot_DependOn_DomainTypes()
    {
        foreach (var module in ModuleNamespaces)
        {
            var result = Types
                .InAssembly(MonolithAssembly)
                .That()
                .ResideInNamespaceStartingWith($"{module}.Contracts")
                .ShouldNot()
                .HaveDependencyOn($"{module}.Domain")
                .GetResult();

            Assert.True(
                result.IsSuccessful,
                $"[{module}.Contracts] exposes domain types — contracts must only contain DTOs and integration events: "
                    + string.Join(", ", result.FailingTypeNames ?? [])
            );
        }
    }

    /// <summary>
    /// All Wolverine handler classes must follow the *Handler naming suffix convention
    /// required by Wolverine for handler discovery.
    /// Covers Features, DomainEventHandlers, and IntegrationEventHandlers namespaces.
    /// </summary>
    [Fact]
    public void Handlers_ShouldHave_HandlerSuffix()
    {
        var handlerNamespaces = new[] { "Features", "DomainEventHandlers", "IntegrationEventHandlers" };

        foreach (var module in ModuleNamespaces)
        {
            foreach (var ns in handlerNamespaces)
            {
                var result = Types
                    .InAssembly(MonolithAssembly)
                    .That()
                    .ResideInNamespaceStartingWith($"{module}.{ns}")
                    .And()
                    .MeetCustomRule(new HasHandleMethodRule())
                    .Should()
                    .HaveNameEndingWith("Handler")
                    .GetResult();

                Assert.True(
                    result.IsSuccessful,
                    $"[{module}.{ns}] contains handler classes not using the *Handler suffix: "
                        + string.Join(", ", result.FailingTypeNames ?? [])
                );
            }
        }
    }
}

/// <summary>
/// NetArchTest custom rule: matches classes that declare a Handle or HandleAsync method,
/// which identifies them as Wolverine handler classes.
/// </summary>
file sealed class HasHandleMethodRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type)
    {
        return type.Methods.Any(m => m.Name is "Handle" or "HandleAsync");
    }
}
