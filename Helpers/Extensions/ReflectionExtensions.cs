using System.Reflection;

namespace OmenTools.Helpers;

public static class ReflectionExtensions
{
    private const BindingFlags ALL_FLAGS      = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private const BindingFlags STATIC_FLAGS   = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    private const BindingFlags INSTANCE_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    extension(object obj)
    {
        public object? GetFieldOrProperty(string name) =>
            obj.GetType().GetField(name, ALL_FLAGS)?.GetValue(obj) ??
            obj.GetType().GetProperty(name, ALL_FLAGS)?.GetValue(obj);

        public T? GetFieldOrProperty<T>(string name) => 
            (T?)obj.GetFieldOrProperty(name);

        public void SetFieldOrProperty(string name, object value)
        {
            var field = obj.GetType().GetField(name, ALL_FLAGS);
            if (field != null)
                field.SetValue(obj, value);
            else
                obj.GetType().GetProperty(name, ALL_FLAGS)?.SetValue(obj, value);
        }

        public object? GetStaticFieldOrProperty(string type, string name) =>
            obj.GetType().Assembly.GetType(type)?.GetField(name, STATIC_FLAGS)?.GetValue(null) ??
            obj.GetType().Assembly.GetType(type)?.GetProperty(name, STATIC_FLAGS)?.GetValue(null);

        public T? GetStaticFieldOrProperty<T>(string type, string name) => 
            (T?)obj.GetStaticFieldOrProperty(type, name);

        public void SetStaticFieldOrProperty(string type, string name, object value)
        {
            var field = obj.GetType().Assembly.GetType(type)?.GetField(name, STATIC_FLAGS);
            if (field != null)
                field.SetValue(null, value);
            else
                obj.GetType().Assembly.GetType(type)?.GetProperty(name, STATIC_FLAGS)?.SetValue(null, value);
        }

        public object? Call(string name, object[] @params, bool matchExactArgumentTypes = false)
        {
            var info = !matchExactArgumentTypes
                           ? obj.GetType().GetMethod(name, ALL_FLAGS)
                           : obj.GetType().GetMethod(name, ALL_FLAGS, @params.Select(x => x.GetType()).ToArray());
            return info?.Invoke(obj, @params);
        }

        public T? Call<T>(string name, object[] @params, bool matchExactArgumentTypes = false) => 
            (T?)obj.Call(name, @params, matchExactArgumentTypes);
    }
}
