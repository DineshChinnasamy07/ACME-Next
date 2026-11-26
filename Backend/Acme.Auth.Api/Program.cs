
/*  Class Name      : Program.cs
 *  Purpose         : Auth API startup, portal database, JWT, and Swagger
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */

using Acme.Auth.Infrastructure.Extensions;
using Acme.Common.Logging;
using Acme.Common.Security;
using Acme.Common.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging
builder.Host.UseAcmeSerilog();

// Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Swagger configuration with JWT
builder.Services.AddAcmeSwaggerWithJwt(
    "Auth",
    "Users",
    "Roles",
    "Rights",
    "Permissions");

// Register Auth services
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddAcmeJwtAuthentication(builder.Configuration);

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
    app.MapOpenApi();

    app.UseSwagger();

    app.UseAcmeSwaggerUI(
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
