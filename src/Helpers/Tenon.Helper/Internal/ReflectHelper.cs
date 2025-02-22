﻿using System.ComponentModel;
using System.Reflection;

namespace Tenon.Helper.Internal;

public static class ReflectHelper
{
    private static readonly BindingFlags BindingFlags;

    static ReflectHelper()
    {
        BindingFlags = BindingFlags.Instance | BindingFlags.Public;
    }


    public static bool Contain<T>(T model, string propertyName)
        where T : class
    {
        Checker.Begin().NotNull(model, nameof(model)).NotNullOrEmpty(propertyName, nameof(propertyName));
        var propertyInfo = model.GetType().GetProperty(propertyName);
        return propertyInfo != null;
    }


    public static T CreateInstance<T>(string fullName, string assemblyName)
        where T : class, new()
    {
        Checker.Begin().NotNullOrEmpty(fullName, nameof(fullName))
            .NotNullOrEmpty(assemblyName, nameof(assemblyName));
        var path = fullName + "," + assemblyName; //命名空间.类型名,程序集
        var loadType = Type.GetType(path); //加载类型
        var instance = Activator.CreateInstance(loadType ?? throw new InvalidOperationException(), true); //根据类型创建实例
        return (T)instance; //类型转换并返回
    }


    public static TV GetFieldValue<T, TV>(T model, string propertyName)
        where T : class
    {
        Checker.Begin().NotNull(model, nameof(model)).NotNullOrEmpty(propertyName, nameof(propertyName));
        var fieldInfo = model.GetType().GetProperty(propertyName, BindingFlags);
        return (TV)fieldInfo?.GetValue(model);
    }


    public static PropertyInfo[] GetPropertyInfo<T>() where T : class
    {
        return typeof(T).GetProperties();
    }


    public static IDictionary<string, string> GetPropertyName<T>() where T : class
    {
        IDictionary<string, string> dict = new Dictionary<string, string>();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var attributes = property.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            dict.Add(property.Name,
                attributes.Length == 0 ? property.Name : ((DisplayNameAttribute)attributes[0]).DisplayName);
        }

        return dict;
    }


    public static Dictionary<string, object> DictionaryFromType<T>(this T model) where T : class
    {
        Checker.Begin().NotNull(model, nameof(model));
        var properties = GetPropertyInfo<T>();
        var dict = new Dictionary<string, object>();

        foreach (var item in properties)
        {
            var attrs = item.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            var attrName = attrs.Length == 0 ? item.Name : ((DisplayNameAttribute)attrs[0]).DisplayName;
            var attrValue = item.GetValue(model, new object[] { });
            dict.Add(attrName, attrValue);
        }

        return dict;
    }


    public static object InvokeMethod(object item, string methodName, object[] args)
    {
        var type = item.GetType();
        return type.InvokeMember(methodName, BindingFlags | BindingFlags.InvokeMethod, null, item, args);
    }

    public static void SetFieldValue<T, TV>(T model, string name, TV fieldValue)
        where T : class
    {
        var fieldInfo = model.GetType().GetField(name, BindingFlags);
        fieldInfo?.SetValue(model, fieldValue);
    }


    public static IEnumerable<T> CreateInterfaceTypeInstances<T>(Assembly? assembly = null) where T : class
    {
        var interfaceType = typeof(T);
        if (!interfaceType.IsInterface) 
            throw new NotSupportedException(nameof(interfaceType));
        if (assembly == null)
            assembly = Assembly.GetCallingAssembly();
        var implementingTypes = GetImplementingTypes(interfaceType, assembly);

        var instances = new List<T>();

        foreach (var type in implementingTypes)
            if (Activator.CreateInstance(type) is T instance)
                instances.Add(instance);

        return instances;
    }

    public static List<Type> GetImplementingTypes(Type interfaceType, Assembly assembly)
    {
        var types = assembly.GetTypes();

        var implementingTypes = types
            .Where(t => interfaceType.IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .ToList();

        return implementingTypes;
    }

    public static IEnumerable<Type> GetDerivedTypes(Type baseType, Assembly assembly)
    {
        var derivedTypes = new List<Type>();
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract);
        derivedTypes.AddRange(types);
        return derivedTypes;
    }

    public static List<T> InstantiateDerivedTypes<T>(IEnumerable<Type> types) where T : class, new()
    {
        var instances = new List<T>();

        foreach (var type in types)
            if (Activator.CreateInstance(type) is T instance)
                instances.Add(instance);

        return instances;
    }

    public static IEnumerable<T> CreateDerivedInstances<T>(Assembly? assembly = null) where T : class, new()
    {
        if (assembly == null)
            assembly = Assembly.GetCallingAssembly();
        var derivedTypes = GetDerivedTypes(typeof(T), assembly);
        return InstantiateDerivedTypes<T>(derivedTypes);
    }
}