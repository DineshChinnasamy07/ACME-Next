using Bosco.Common.Swagger;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    $"ocelot.{builder.Environment.EnvironmentName}.json",
    optional: false,
    reloadOnChange: true);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddBoscoSwaggerWithJwt();
builder.Services.AddOcelot(builder.Configuration);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .WriteTo.File(
            "serilogs/log-.txt",
            rollingInterval: RollingInterval.Day);
});

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger"));

await app.UseOcelot();

app.Run();