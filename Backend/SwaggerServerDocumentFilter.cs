using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Backend;

public sealed class SwaggerServerDocumentFilter : IDocumentFilter
{
    // Use relative server URL so Swagger UI uses the same host/port you are viewing.
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Servers = new List<OpenApiServer>
        {
            new OpenApiServer { Url = "/" }
        };
    }
}

