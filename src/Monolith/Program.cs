using JasperFx;
using JasperFx.Resources;
using Microsoft.AspNetCore.Mvc.Controllers;
using Monolith.Modules.Orders;
using Monolith.Modules.Users;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder
    .Services.AddControllers()
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
    // enable PostgreSQL backed partitioning for the inbox table as an optimization.
    opts.Durability.EnableInboxPartitioning = true;

    // Make sure that multiple event handlers for the same event are executed in separate transactions and errors in one handler don't affect the others
    opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
    opts.Policies.AutoApplyTransactions();

    // Where and how to store messages as part of the transactional inbox/outbox
    opts.PersistMessagesWithPostgresql(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.")
    );

    // Adding EF Core transactional middleware, saga support,
    // and EF Core support for Wolverine storage operations
    opts.UseEntityFrameworkCoreTransactions();
});
builder.Host.UseResourceSetupOnStartup();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();

return await app.RunJasperFxCommands(args);
