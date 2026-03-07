using JasperFx;
using Microsoft.AspNetCore.Mvc.Controllers;
using Monolith.Bootstrapper;
using Monolith.Modules.Orders;
using Monolith.Modules.Users;
using Scalar.AspNetCore;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddApplicationPart(typeof(OrdersModule).Assembly)
    .AddApplicationPart(typeof(UsersModule).Assembly)
    .ConfigureApplicationPartManager(manager =>
    {
        var defaultProvider = manager.FeatureProviders.OfType<ControllerFeatureProvider>().First();
        manager.FeatureProviders.Remove(defaultProvider);
        manager.FeatureProviders.Add(new InternalControllerFeatureProvider());
    });

builder.Services.AddOrdersModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);

builder.Host.UseWolverine(opts =>
{
    opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
    opts.Policies.AutoApplyTransactions();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();

return await app.RunJasperFxCommands(args);