/*  Class Name      : Program.cs
 *  Purpose         : Organization API startup, EF PostgreSQL and Swagger
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Acme.Common.Logging;
using Acme.Common.Security;
using Acme.Common.Swagger;
using Acme.Organization.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseAcmeSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddAcmeSwaggerWithJwt("HeadOffice", "Country", "BranchOffice", "BranchLocation");
builder.Services.AddAcmeJwtAuthentication(builder.Configuration);
builder.Services.AddOrganizationServices(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseAcmeSwaggerUI("HeadOffice", "Country", "BranchOffice", "BranchLocation");
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
