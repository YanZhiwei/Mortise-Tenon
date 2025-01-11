using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Tenon.AspNetCore.OpenApi.Extensions.Transformers;

public sealed class CommaDelimitedArrayDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string ArrayDescription = "(A comma-separated array, e.g., 1,2,3)";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var arraySchemas = document.Components.Schemas
            .SelectMany(schema => schema.Value.Properties
                .Where(property => property.Value.Type == "array"))
            .ToList();

        foreach (var property in arraySchemas)
            property.Value.Description = AppendDescription(property.Value.Description, ArrayDescription);

        return Task.CompletedTask;
    }

    private string AppendDescription(string? existingDescription, string appendText)
    {
        if (string.IsNullOrWhiteSpace(existingDescription)) return appendText;

        if (!existingDescription.Contains(appendText)) return $"{existingDescription}\n{appendText}";

        return existingDescription;
    }
}