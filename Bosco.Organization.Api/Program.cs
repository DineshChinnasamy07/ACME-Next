/*  Class Name      : Program.cs
 *  Purpose         : Organization API startup, EF PostgreSQL and Swagger
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Bosco.Common.Logging;
using Bosco.Common.Security;
using Bosco.Common.Swagger;
using Bosco.Organization.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseBoscoSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddBoscoSwaggerWithJwt("HeadOffice", "Country", "BranchOffice");
builder.Services.AddBoscoJwtAuthentication(builder.Configuration);
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
    app.UseBoscoSwaggerBasicAuth();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseBoscoSwaggerUI("HeadOffice", "Country", "BranchOffice");
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
