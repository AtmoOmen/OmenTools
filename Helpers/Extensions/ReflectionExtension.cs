using System.Reflection;

namespace OmenTools.Helpers;

public static class ReflectionExtension
{
    private const BindingFlags AllFlags      = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private const BindingFlags StaticFlags   = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    private const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public static object? GetFoP(this object obj, string name) =>
        obj.GetType().GetField(name, AllFlags)?.GetValue(obj) ??
        obj.GetType().GetProperty(name, AllFlags)?.GetValue(obj);

    public static T? GetFoP<T>(this object obj, string name) => 
        (T?)GetFoP(obj, name);

    public static void SetFoP(this object obj, string name, object value)
    {
        var field = obj.GetType().GetField(name, AllFlags);
        if (field != null)
            field.SetValue(obj, value);
        else
            obj.GetType().GetProperty(name, AllFlags)?.SetValue(obj, value);
    }

    public static object? GetStaticFoP(this object obj, string type, string name) =>
        obj.GetType().Assembly.GetType(type)?.GetField(name, StaticFlags)?.GetValue(null) ??
        obj.GetType().Assembly.GetType(type)?.GetProperty(name, StaticFlags)?.GetValue(null);

    public static T? GetStaticFoP<T>(this object obj, string type, string name) => 
        (T?)GetStaticFoP(obj, type, name);

    public static void SetStaticFoP(this object obj, string type, string name, object value)
    {
        var field = obj.GetType().Assembly.GetType(type)?.GetField(name, StaticFlags);
        if (field != null)
            field.SetValue(null, value);
        else
            obj.GetType().Assembly.GetType(type)?.GetProperty(name, StaticFlags)?.SetValue(null, value);
    }

    /// <returns>Object returned by the target method</returns>
    public static object? Call(this object obj, string name, object[] @params, bool matchExactArgumentTypes = false)
    {
        var info = !matchExactArgumentTypes
                       ? obj.GetType().GetMethod(name, AllFlags)
                       : obj.GetType().GetMethod(name, AllFlags, @params.Select(x => x.GetType()).ToArray());
        return info?.Invoke(obj, @params);
    }

    public static T? Call<T>(this object obj, string name, object[] @params, bool matchExactArgumentTypes = false) 
        => (T?)Call(obj, name, @params, matchExactArgumentTypes);
}
