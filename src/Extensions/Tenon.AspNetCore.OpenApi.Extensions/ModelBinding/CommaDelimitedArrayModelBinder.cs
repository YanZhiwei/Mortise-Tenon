using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Tenon.AspNetCore.OpenApi.Extensions.ModelBinding;

/// <summary>
/// 逗号分隔数组模型绑定器
/// </summary>
public class CommaDelimitedArrayModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var elementType = bindingContext.ModelType.GetElementType() ?? bindingContext.ModelType.GenericTypeArguments[0];
        var converter = TypeDescriptor.GetConverter(elementType);

        try
        {
            var values = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Select(x => converter.ConvertFromString(x))
                .ToArray();

            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);

            bindingContext.Result = ModelBindingResult.Success(typedValues);
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"Could not convert value to {elementType.Name}. Error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
} 