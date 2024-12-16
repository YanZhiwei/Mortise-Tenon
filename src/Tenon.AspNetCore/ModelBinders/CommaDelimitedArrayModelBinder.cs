using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Tenon.AspNetCore.ModelBinders;

public class CommaDelimitedArrayModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(value))
            return Task.CompletedTask;

        Type? elementType = bindingContext.ModelType.GetElementType() 
                            ?? bindingContext.ModelType.GenericTypeArguments.FirstOrDefault();

        if (elementType == null)
            return Task.CompletedTask;

        try
        {
            var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(x => Convert.ChangeType(x.Trim(), elementType))
                              .ToArray();

            var typedValues = Array.CreateInstance(elementType, values.Length);
            Array.Copy(values, typedValues, values.Length);

            bindingContext.Result = ModelBindingResult.Success(typedValues);
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, ex.Message);
        }

        return Task.CompletedTask;
    }
}