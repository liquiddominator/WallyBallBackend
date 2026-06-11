using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WallyBallBackend.Api.OpenApi;

public sealed class AuthorizeDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var bearerRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", swaggerDoc, null)] = new List<string>()
        };

        foreach (var (path, pathItem) in swaggerDoc.Paths)
        {
            if (pathItem.Operations is null)
            {
                continue;
            }

            foreach (var (method, operation) in pathItem.Operations)
            {
                operation.Security = [bearerRequirement];
            }
        }
    }
}
