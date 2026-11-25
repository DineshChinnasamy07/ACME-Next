using Acme.Common.Swagger;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAcmeSwaggerWithJwt();

// YARP reverse proxy: routes and clusters come from the "ReverseProxy" section
// of appsettings.{Environment}.json (reloaded automatically when the file changes).
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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

app.MapReverseProxy();

app.Run();