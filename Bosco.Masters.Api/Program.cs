/*  Class Name      : Program.cs
 *  Purpose         : Masters API startup, EF PostgreSQL, JWT and Swagger
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Common.Logging;
using Bosco.Common.Security;
using Bosco.Common.Swagger;
using Bosco.Masters.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseBoscoSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddBoscoSwaggerWithJwt("HeadOfficeMasters", "FinanceMasters", "AssetMasters", "PayrollMasters");
builder.Services.AddBoscoJwtAuthentication(builder.Configuration);
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
    app.UseBoscoSwaggerBasicAuth();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseBoscoSwaggerUI("HeadOfficeMasters", "FinanceMasters", "AssetMasters", "PayrollMasters");
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
