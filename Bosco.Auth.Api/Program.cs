
/*  Class Name      : Program.cs
 *  Purpose         : Auth API startup, portal database, JWT, and Swagger
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */

using Bosco.Auth.Infrastructure.Extensions;
using Bosco.Common.Logging;
using Bosco.Common.Security;
using Bosco.Common.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging
builder.Host.UseBoscoSerilog();

// Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Swagger configuration with JWT
builder.Services.AddBoscoSwaggerWithJwt(
    "Auth",
    "Users",
    "Roles",
    "Rights",
    "Permissions");

// Register Auth services
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddBoscoJwtAuthentication(builder.Configuration);

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure Swagger
if (app.Environment.IsDevelopment())
{
    app.UseBoscoSwaggerBasicAuth();
    app.MapOpenApi();

    app.UseSwagger();

    app.UseBoscoSwaggerUI(
        "Auth",
        "Users",
        "Roles",
        "Rights",
        "Permissions");
}

app.UseHttpsRedirection();

// CORS
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Run application
app.Run();
