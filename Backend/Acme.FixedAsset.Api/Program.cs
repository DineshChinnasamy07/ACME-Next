/*  Class Name      : Program.cs
 *  Purpose         : Fixed Asset API startup, PostgreSQL connection and Swagger
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */

using Acme.Common.Logging;
using Acme.Common.Security;
using Acme.Common.Swagger;
using Acme.Data;
using Acme.FixedAsset.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure shared Serilog logging for the Fixed Asset API
builder.Host.UseAcmeSerilog();

// Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddAcmeSwaggerWithJwt();
builder.Services.AddAcmeJwtAuthentication(builder.Configuration);

// Configure database connection
builder.Services.AddAcmeDatabase(builder.Configuration);
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
app.UseAuthentication();
app.UseAuthorization();

// Map API controllers
app.MapControllers();

app.Run();