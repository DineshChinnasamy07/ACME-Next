/*  Class Name      : Program.cs
 *  Purpose         : Masters API startup, EF PostgreSQL, JWT and Swagger
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Common.Logging;
using Acme.Common.Security;
using Acme.Common.Swagger;
using Acme.Masters.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseAcmeSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddAcmeSwaggerWithJwt("HeadOfficeMasters", "FinanceMasters", "AssetMasters", "PayrollMasters");
builder.Services.AddAcmeJwtAuthentication(builder.Configuration);
builder.Services.AddMastersServices(builder.Configuration);
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
    app.UseAcmeSwaggerUI("HeadOfficeMasters", "FinanceMasters", "AssetMasters", "PayrollMasters");
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
