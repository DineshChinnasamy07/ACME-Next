/*  Class Name      : SwaggerExtensions.cs
 *  Purpose         : Shared Swagger JWT bearer Authorize button
 *  Author          : Dinesh C
 *  Created on      : 26-Oct-2026
 */
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bosco.Common.Swagger
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddBoscoSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the access token only. Swagger adds the Bearer prefix."
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });
            });
            return services;
        }
    }
}
