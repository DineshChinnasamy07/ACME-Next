/*  Class Name      : Program.cs
 *  Purpose         : Budget API startup, PostgreSQL connection and Swagger
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Bosco.Budget.Infrastructure.Extensions;
using Bosco.Common.Logging;
using Bosco.Common.Swagger;
using Bosco.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseBoscoSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddBoscoSwaggerWithJwt();
builder.Services.AddBoscoDatabase(builder.Configuration);
builder.Services.AddBudgetServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
