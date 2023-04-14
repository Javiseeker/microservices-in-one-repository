using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace AMR.Shared.Swagger;

public static class SwaggerConfig
{
    public static void UseSwagger<TProgram>(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerOptions>();
        services.AddSwaggerGen(option =>
        {
            // add a custom operation filter which sets default values
            option.OperationFilter<SwaggerDefaultValues>();

            try
            {
                var basePath = AppContext.BaseDirectory;
                var fileName = typeof(TProgram).Assembly.GetName().Name + ".xml";
                var xmlCommentsFilePath = Path.Combine(basePath, fileName);

                //Set the comments path for the swagger json and ui.
                option.IncludeXmlComments(xmlCommentsFilePath);
            }
            catch (Exception _)
            {
                // TODO: need to investigate why it's crashing sometimes.
            }

            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });

        });
    }

    public static void UseSwaggerUi(this WebApplication app)
    {
        // TODO: need to check how to add the custom css here. Not working with docker-compose

        try
        {
            var path = Path.Combine(Constants.AssemblyDirectory, "Swagger");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = "/swagger-ui"
            });
        }
        catch (Exception _)
        {
            // For now ignore this exception as the swagger styling components will work eventually.
        }

        app.UseSwagger();

        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwaggerUI(c =>
        {
            //c.InjectStylesheet("/swagger-ui/custom.css");

            foreach (var description in provider.ApiVersionDescriptions)
            {
                c.SwaggerEndpoint($"../swagger/{description.GroupName}/swagger.json", description.GroupName);
            }
        });

    }


}
