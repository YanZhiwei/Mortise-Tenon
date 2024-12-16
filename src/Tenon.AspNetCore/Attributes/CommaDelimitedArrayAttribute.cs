using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tenon.AspNetCore.ModelBinders;

namespace Tenon.AspNetCore.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class CommaDelimitedArrayAttribute : Attribute, IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (context.Metadata.ModelType.IsArray) return new CommaDelimitedArrayModelBinder();

        return null;
    }
}