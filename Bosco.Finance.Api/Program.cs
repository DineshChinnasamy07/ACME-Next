/*  Class Name      : Program.cs
 *  Purpose         : Finance API startup, PostgreSQL connection and Swagger
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using Bosco.Common.Logging;
using Bosco.Common.Security;
using Bosco.Common.Swagger;
using Bosco.Data;
using Bosco.Finance.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseBoscoSerilog(); //this Inajection is log Related Once 

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddBoscoSwaggerWithJwt();
builder.Services.AddBoscoJwtAuthentication(builder.Configuration);
builder.Services.AddBoscoDatabase(builder.Configuration);
builder.Services.AddFinanceServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseBoscoSwaggerBasicAuth();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
