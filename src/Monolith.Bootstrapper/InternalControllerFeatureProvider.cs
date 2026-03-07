using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Monolith.Bootstrapper;

/// <summary>
/// Overrides the default controller feature provider to also discover <c>internal</c> controller types.
/// This allows module controllers to remain internal (inaccessible to other modules)
/// while still being registered by MVC when added via <c>AddApplicationPart</c>.
/// </summary>
internal sealed class InternalControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo) =>
        typeInfo.IsClass &&
        !typeInfo.IsAbstract &&
        !typeInfo.ContainsGenericParameters &&
        !typeInfo.IsDefined(typeof(NonControllerAttribute)) &&
        (typeInfo.IsDefined(typeof(ControllerAttribute), true) ||
         typeof(ControllerBase).IsAssignableFrom(typeInfo.AsType()));
}
