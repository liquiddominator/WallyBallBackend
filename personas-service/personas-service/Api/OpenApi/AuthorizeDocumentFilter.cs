using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PersonasService.Api.OpenApi;

public sealed class AuthorizeDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Paths is null)
        {
            return;
        }

        var operations = swaggerDoc.Paths.Values
            .Where(path => path.Operations is not null)
            .SelectMany(path => path.Operations!.Values);

        foreach (var operation in operations)
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", swaggerDoc)] = []
            });
        }
    }
}
