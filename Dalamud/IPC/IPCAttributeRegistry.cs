using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using OmenTools.Dalamud.Abstractions;
using OmenTools.Dalamud.Attributes;

namespace OmenTools.Dalamud;

public static class IPCAttributeRegistry
{
    private const string REGISTER_ACTION_METHOD_NAME = "RegisterAction";
    private const string REGISTER_FUNC_METHOD_NAME   = "RegisterFunc";

    private static readonly ConditionalWeakTable<object, List<ICallGateProvider>> RegisteredProviders = [];

    private static readonly Dictionary<int, MethodInfo> GetIPCProviderMethods =
        typeof(IDalamudPluginInterface)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m is { Name: nameof(IDalamudPluginInterface.GetIpcProvider), IsGenericMethodDefinition: true })
            .GroupBy(m => m.GetGenericArguments().Length)
            .ToDictionary(group => group.Key, group => group.First());

    private static readonly Type[] FuncDelegateTypes =
    [
        typeof(Func<>),
        typeof(Func<,>),
        typeof(Func<,,>),
        typeof(Func<,,,>),
        typeof(Func<,,,,>),
        typeof(Func<,,,,,>),
        typeof(Func<,,,,,,>),
        typeof(Func<,,,,,,,>),
        typeof(Func<,,,,,,,,>)
    ];

    private static readonly Type[] ActionDelegateTypes =
    [
        typeof(Action),
        typeof(Action<>),
        typeof(Action<,>),
        typeof(Action<,,>),
        typeof(Action<,,,>),
        typeof(Action<,,,,>),
        typeof(Action<,,,,,>),
        typeof(Action<,,,,,,>),
        typeof(Action<,,,,,,,>),
        typeof(Action<,,,,,,,,>)
    ];

    private static readonly MethodInfo CreateValueProviderMethod =
        typeof(IPCAttributeRegistry).GetMethod(nameof(CreateValueProviderCore), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo CreateDefaultValueFactoryMethod =
        typeof(IPCAttributeRegistry).GetMethod(nameof(CreateDefaultValueFactoryCore), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static void RegObjectIPCs(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        RegisterIPCs(instance, instance.GetType(), instance, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    }

    public static void UnregObjectIPCs(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        UnregisterIPCs(instance);
    }

    public static void RegStaticIPCs(Type staticType)
    {
        ArgumentNullException.ThrowIfNull(staticType);
        RegisterIPCs(staticType, staticType, null, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
    }

    public static void UnregStaticIPCs(Type staticType)
    {
        ArgumentNullException.ThrowIfNull(staticType);
        UnregisterIPCs(staticType);
    }

    private static void RegisterIPCs(object key, Type declaringType, object? instance, BindingFlags flags)
    {
        UnregisterIPCs(key);

        var providers = new List<ICallGateProvider>();

        foreach (var member in declaringType.GetMembers(flags))
        {
            if (member.GetCustomAttribute<IPCProviderAttribute>() is { } providerAttr &&
                CreateProvider(instance, member, providerAttr) is { } provider)
                providers.Add(provider);

            if (member.GetCustomAttribute<IPCSubscriberAttribute>() is { } subscriberAttr)
                _ = CreateSubscriber(instance, member, subscriberAttr);
        }

        if (providers.Count != 0)
            RegisteredProviders.Add(key, providers);
    }

    private static void UnregisterIPCs(object key)
    {
        if (!RegisteredProviders.TryGetValue(key, out var providers))
            return;

        foreach (var provider in providers)
        {
            try
            {
                provider.UnregisterAction();
                provider.UnregisterFunc();
            }
            catch (Exception ex)
            {
                DLog.Error("卸载 IPC Provider 时发生错误", ex);
            }
        }

        RegisteredProviders.Remove(key);
    }

    private static ICallGateProvider? CreateProvider(object? instance, MemberInfo member, IPCProviderAttribute attr)
    {
        try
        {
            return member switch
            {
                MethodInfo method                       => CreateMethodProvider(instance, method, attr),
                PropertyInfo { CanRead: true } property => CreateValueProvider(instance, property, property.PropertyType, attr.IPCName),
                FieldInfo field                         => CreateValueProvider(instance, field,    field.FieldType,       attr.IPCName),
                _                                       => null
            };
        }
        catch (Exception ex)
        {
            DLog.Error($"创建 Provider 失败: {attr.IPCName}", ex);
            return null;
        }
    }

    private static ICallGateProvider? CreateMethodProvider(object? instance, MethodInfo method, IPCProviderAttribute attr)
    {
        var    parameterTypes = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
        var    isAction       = method.ReturnType == typeof(void);
        Type[] providerTypes;
        Type[] delegateTypes;

        if (isAction)
        {
            providerTypes = [.. parameterTypes, typeof(object)];
            delegateTypes = parameterTypes;
        }
        else
        {
            providerTypes = [.. parameterTypes, method.ReturnType];
            delegateTypes = [.. parameterTypes, method.ReturnType];
        }

        var provider = CreateProviderInstance(attr.IPCName, providerTypes);
        if (provider is not ICallGateProvider callGateProvider)
            return null;

        var delegateType = CreateDelegateType(isAction, delegateTypes);

        if (delegateType == null)
        {
            DLog.Error($"创建 Provider 失败: {attr.IPCName} - 当前仅支持最多 8 个参数的委托");
            return null;
        }

        var callback = CreateMethodDelegate(method, instance, delegateType);
        if (callback == null)
            return null;

        RegisterProviderCallback(provider, isAction ? REGISTER_ACTION_METHOD_NAME : REGISTER_FUNC_METHOD_NAME, callback, attr.IPCName);
        return callGateProvider;
    }

    private static object? CreateProviderInstance(string ipcName, Type[] genericTypes)
    {
        if (!GetIPCProviderMethods.TryGetValue(genericTypes.Length, out var methodDefinition))
        {
            DLog.Error($"创建 Provider 失败: {ipcName} - 找不到具有 {genericTypes.Length} 个泛型参数的 GetIpcProvider 方法");
            return null;
        }

        return methodDefinition.MakeGenericMethod(genericTypes).Invoke(DService.Instance().PI, [ipcName]);
    }

    private static Type? CreateDelegateType(bool isAction, Type[] genericTypes)
    {
        if (isAction)
        {
            if (genericTypes.Length >= ActionDelegateTypes.Length)
                return null;

            return genericTypes.Length == 0 ? ActionDelegateTypes[0] : ActionDelegateTypes[genericTypes.Length].MakeGenericType(genericTypes);
        }

        if (genericTypes.Length == 0 || genericTypes.Length > FuncDelegateTypes.Length)
            return null;

        return FuncDelegateTypes[genericTypes.Length - 1].MakeGenericType(genericTypes);
    }

    private static Delegate? CreateMethodDelegate(MethodInfo method, object? instance, Type delegateType)
    {
        if (!method.IsStatic && instance == null)
        {
            DLog.Error($"创建委托失败: {method.DeclaringType?.FullName}.{method.Name} 需要实例对象");
            return null;
        }

        try
        {
            return method.IsStatic ? method.CreateDelegate(delegateType) : method.CreateDelegate(delegateType, instance);
        }
        catch (Exception ex)
        {
            DLog.Error($"创建方法委托失败: {method.DeclaringType?.FullName}.{method.Name}", ex);
            return null;
        }
    }

    private static void RegisterProviderCallback(object provider, string methodName, Delegate callback, string ipcName)
    {
        var registerMethod = provider.GetType().GetMethod(methodName);
        if (registerMethod == null)
            throw new MissingMethodException(provider.GetType().FullName, methodName);

        try
        {
            registerMethod.Invoke(provider, [callback]);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"注册 IPC {ipcName} 的 {methodName} 回调失败", ex);
        }
    }

    private static ICallGateProvider? CreateValueProvider(object? instance, MemberInfo member, Type valueType, string ipcName)
    {
        try
        {
            return (ICallGateProvider?)CreateValueProviderMethod.MakeGenericMethod(valueType).Invoke(null, [instance, member, ipcName]);
        }
        catch (Exception ex)
        {
            DLog.Error($"创建值 Provider 失败: {ipcName}", ex);
            return null;
        }
    }

    private static ICallGateProvider CreateValueProviderCore<T>(object? instance, MemberInfo member, string ipcName)
    {
        var provider = DService.Instance().PI.GetIpcProvider<T>(ipcName);
        provider.RegisterFunc(CreateMemberGetter<T>(member, instance));
        return provider;
    }

    private static Func<T> CreateMemberGetter<T>(MemberInfo member, object? instance) => member switch
    {
        PropertyInfo property => () => (T)property.GetValue(instance)!,
        FieldInfo field       => () => (T)field.GetValue(instance)!,
        _                     => throw new ArgumentOutOfRangeException(nameof(member), "仅支持字段或属性作为值 Provider")
    };

    private static bool CreateSubscriber(object? instance, MemberInfo member, IPCSubscriberAttribute attr)
    {
        var (isValid, errorMessage) = attr.Validate();

        if (!isValid)
        {
            DLog.Error($"创建 Subscriber 失败: {errorMessage}");
            return false;
        }

        try
        {
            return member is FieldInfo field && CreateFieldSubscriber(instance, field, attr);
        }
        catch (Exception ex)
        {
            DLog.Error($"创建 Subscriber 失败: {attr.IPCName}", ex);
            return false;
        }
    }

    private static bool CreateFieldSubscriber(object? instance, FieldInfo field, IPCSubscriberAttribute attr)
    {
        if (!IsIPCSubscriberField(field.FieldType))
            return false;

        var defaultValueFactory = CreateDefaultValueFactory(field.FieldType, attr);
        var subscriber          = Activator.CreateInstance(field.FieldType, attr.IPCName, defaultValueFactory);

        if (subscriber is not IPCSubscriberBase ipcSubscriber)
            return false;

        ipcSubscriber.AutoInitialize = attr.AutoInitialize;
        field.SetValue(instance, subscriber);

        if (attr.AutoInitialize)
            _ = ipcSubscriber.Initialize();

        return true;
    }

    private static bool IsIPCSubscriberField(Type fieldType)
    {
        if (!fieldType.IsGenericType)
            return false;

        var genericDefinition = fieldType.GetGenericTypeDefinition();
        return genericDefinition.Namespace == typeof(IPCSubscriber<int>).Namespace &&
               genericDefinition.Name.StartsWith("IPCSubscriber`", StringComparison.Ordinal);
    }

    private static object? CreateDefaultValueFactory(Type subscriberType, IPCSubscriberAttribute attr)
    {
        if (attr.DefaultValue == null || !subscriberType.IsGenericType)
            return null;

        var returnType = subscriberType.GetGenericArguments()[^1];

        try
        {
            return CreateDefaultValueFactoryMethod.MakeGenericMethod(returnType).Invoke(null, [attr.DefaultValue, attr.IPCName]);
        }
        catch (TargetInvocationException ex)
        {
            DLog.Error($"解析默认值失败: DefaultValue='{attr.DefaultValue}', IPCName='{attr.IPCName}'", ex.InnerException ?? ex);
            return null;
        }
        catch (Exception ex)
        {
            DLog.Error($"解析默认值失败: DefaultValue='{attr.DefaultValue}', IPCName='{attr.IPCName}'", ex);
            return null;
        }
    }

    private static Func<T> CreateDefaultValueFactoryCore<T>(string rawValue, string ipcName)
    {
        var defaultValue = ConvertDefaultValue<T>(rawValue, ipcName);
        return () => defaultValue;
    }

    private static T ConvertDefaultValue<T>(string rawValue, string ipcName)
    {
        try
        {
            var targetType   = typeof(T);
            var nullableType = Nullable.GetUnderlyingType(targetType);
            var actualType   = nullableType ?? targetType;

            var convertedValue = actualType switch
            {
                _ when actualType == typeof(string)   => rawValue,
                _ when actualType == typeof(bool)     => bool.Parse(rawValue),
                _ when actualType == typeof(Guid)     => Guid.Parse(rawValue),
                _ when actualType == typeof(TimeSpan) => TimeSpan.Parse(rawValue, CultureInfo.InvariantCulture),
                _ when actualType == typeof(DateTime) => DateTime.Parse(rawValue, CultureInfo.InvariantCulture),
                _ when actualType == typeof(DateOnly) => DateOnly.Parse(rawValue, CultureInfo.InvariantCulture),
                _ when actualType == typeof(TimeOnly) => TimeOnly.Parse(rawValue, CultureInfo.InvariantCulture),
                _ when actualType == typeof(Version)  => Version.Parse(rawValue),
                _ when actualType == typeof(Uri)      => new Uri(rawValue, UriKind.RelativeOrAbsolute),
                _ when actualType == typeof(object)   => rawValue,
                _ when actualType.IsEnum              => Enum.Parse(actualType, rawValue, true),
                _                                     => Convert.ChangeType(rawValue, actualType, CultureInfo.InvariantCulture)
            };

            if (nullableType != null)
                return (T)Activator.CreateInstance(targetType, convertedValue)!;

            return (T)convertedValue;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"IPC {ipcName} 的默认值 '{rawValue}' 无法转换为 {typeof(T).Name}", ex);
        }
    }
}
