/*  Class Name      : Program.cs
 *  Purpose         : Payroll API startup, PostgreSQL connection and Swagger
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using Acme.Common.Logging;
using Acme.Common.Security;
using Acme.Common.Swagger;
using Acme.Data;
using Acme.Payroll.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseAcmeSerilog(); // this extension is Add for Serilog Purpose //Added By Dinesh 

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddAcmeSwaggerWithJwt();
builder.Services.AddAcmeJwtAuthentication(builder.Configuration);
builder.Services.AddAcmeDatabase(builder.Configuration);
builder.Services.AddPayrollServices();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
