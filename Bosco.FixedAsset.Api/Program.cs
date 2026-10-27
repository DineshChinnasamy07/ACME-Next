/*  Class Name      : Program.cs
 *  Purpose         : Fixed Asset API startup, PostgreSQL connection and Swagger
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */

using Bosco.Common.Logging;
using Bosco.Common.Swagger;
using Bosco.Data;
using Bosco.FixedAsset.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure shared Serilog logging for the Fixed Asset API
builder.Host.UseBoscoSerilog();

// Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddBoscoSwaggerWithJwt();

// Configure database connection
builder.Services.AddBoscoDatabase(builder.Configuration);
builder.Services.AddFixedAssetServices();

var app = builder.Build();

// Log application startup
app.Logger.LogInformation("Fixed Asset API started successfully");

// Log incoming HTTP requests
app.UseSerilogRequestLogging();

// Swagger configuration
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseHttpsRedirection();
app.UseAuthorization();

// Map API controllers
app.MapControllers();

app.Run();