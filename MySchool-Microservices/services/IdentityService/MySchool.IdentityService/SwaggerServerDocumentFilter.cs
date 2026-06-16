using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MySchool.IdentityService;

/// <summary>Use relative server URL so Swagger works through the API gateway.</summary>
public sealed class SwaggerServerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Servers = new List<OpenApiServer>
        {
            new() { Url = "/" }
        };
    }
}
