/*  Class Name      : SwaggerExtensions.cs
 *  Purpose         : Shared Swagger JWT, XML docs, and definition-per-controller groups
 *  Author          : Dinesh C
 *  Created on      : 26-Oct-2026
 */
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bosco.Common.Swagger
{
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Registers Swagger with JWT. Pass document names to split controllers in "Select a definition".
        /// Controllers must set [ApiExplorerSettings(GroupName = "...")] to match a document name.
        /// </summary>
        public static IServiceCollection AddBoscoSwaggerWithJwt(this IServiceCollection services, params string[] documentNames)
        {
            string[] docs = NormalizeDocuments(documentNames);

            services.AddSwaggerGen(options =>
            {
                foreach (string doc in docs)
                {
                    options.SwaggerDoc(doc, new OpenApiInfo
                    {
                        Title = doc,
                        Version = "v1"
                    });
                }

                options.DocInclusionPredicate((docName, apiDescription) =>
                {
                    string group = string.IsNullOrWhiteSpace(apiDescription.GroupName)
                        ? docs[0]
                        : apiDescription.GroupName;
                    return string.Equals(docName, group, StringComparison.OrdinalIgnoreCase);
                });

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

                IncludeEntryAssemblyXmlComments(options);
            });
            return services;
        }

        /// <summary>
        /// Swagger UI dropdown entries — same document names as AddBoscoSwaggerWithJwt.
        /// </summary>
        public static IApplicationBuilder UseBoscoSwaggerUI(this IApplicationBuilder app, params string[] documentNames)
        {
            string[] docs = NormalizeDocuments(documentNames);
            app.UseSwaggerUI(options =>
            {
                foreach (string doc in docs)
                {
                    options.SwaggerEndpoint("/swagger/" + doc + "/swagger.json", doc);
                }
            });
            return app;
        }

        private static string[] NormalizeDocuments(string[] documentNames)
        {
            if (documentNames == null || documentNames.Length == 0)
            {
                return new[] { "v1" };
            }

            return documentNames;
        }

        private static void IncludeEntryAssemblyXmlComments(SwaggerGenOptions options)
        {
            Assembly entry = Assembly.GetEntryAssembly();
            if (entry == null)
            {
                return;
            }

            string name = entry.GetName().Name;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            string xmlPath = Path.Combine(AppContext.BaseDirectory, name + ".xml");
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        }
    }
}
