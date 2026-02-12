using System.Reflection;
using System.Runtime.CompilerServices;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace OmenTools.Infos;

public static class IPCAttributeRegistry
{
    private static readonly ConditionalWeakTable<object, List<ICallGateProvider>> RegisteredProviders   = [];
    private static readonly ConditionalWeakTable<object, List<string>>            RegisteredSubscribers = [];

    private static readonly Type[] FuncTypes =
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

    private static readonly Type[] ActionTypes =
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

    public static void RegObjectIPCs(object instance)
    {
        if (instance == null) return;

        var providers      = new List<ICallGateProvider>();
        var subscribers    = new List<string>();
        var type           = instance as Type ?? instance.GetType();
        var actualInstance = instance is Type ? null : instance;

        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            var providerAttr = member.GetCustomAttribute<IPCProviderAttribute>();
            if (providerAttr != null)
            {
                var provider = CreateProvider(actualInstance, member, providerAttr);
                if (provider != null)
                    providers.Add(provider);
            }

            var subscriberAttr = member.GetCustomAttribute<IPCSubscriberAttribute>();
            if (subscriberAttr != null)
            {
                if (CreateSubscriber(actualInstance, member, subscriberAttr))
                    subscribers.Add(subscriberAttr.IPCName);
            }
        }

        if (providers.Count != 0)
            RegisteredProviders.TryAdd(instance, providers);
        if (subscribers.Count != 0)
            RegisteredSubscribers.TryAdd(instance, subscribers);
    }

    public static void UnregObjectIPCs(object instance)
    {
        if (RegisteredProviders.TryGetValue(instance, out var providers))
        {
            foreach (var provider in providers)
            {
                try
                {
                    provider.UnregisterAction();
                    provider.UnregisterFunc();
                }
                catch (Exception ex)
                {
                    Error("卸载 IPC Provider 时发生错误", ex);
                }
            }
        }

        RegisteredProviders.Remove(instance);
        RegisteredSubscribers.Remove(instance);
    }

    public static void RegStaticIPCs(Type staticType)
    {
        if (staticType == null) return;

        var providers   = new List<ICallGateProvider>();
        var subscribers = new List<string>();

        foreach (var member in staticType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            var providerAttr = member.GetCustomAttribute<IPCProviderAttribute>();

            if (providerAttr != null)
            {
                var provider = CreateProvider(null, member, providerAttr);
                if (provider != null)
                    providers.Add(provider);
            }

            var subscriberAttr = member.GetCustomAttribute<IPCSubscriberAttribute>();

            if (subscriberAttr != null)
            {
                if (CreateSubscriber(null, member, subscriberAttr))
                    subscribers.Add(subscriberAttr.IPCName);
            }
        }

        if (providers.Count != 0)
            RegisteredProviders.TryAdd(staticType, providers);
        if (subscribers.Count != 0)
            RegisteredSubscribers.TryAdd(staticType, subscribers);
    }

    public static void UnregStaticIPCs(Type staticType)
    {
        if (RegisteredProviders.TryGetValue(staticType, out var providers))
        {
            foreach (var provider in providers)
            {
                try
                {
                    provider.UnregisterAction();
                    provider.UnregisterFunc();
                }
                catch (Exception ex)
                {
                    Error("卸载静态 IPC Provider 时发生错误", ex);
                }
            }
        }

        RegisteredProviders.Remove(staticType);
        RegisteredSubscribers.Remove(staticType);
    }

    private static ICallGateProvider? CreateProvider(object? instance, MemberInfo member, IPCProviderAttribute attr)
    {
        try
        {
            return member switch
            {
                MethodInfo method                       => CreateMethodProvider(instance, method, attr),
                PropertyInfo { CanRead: true } property => CreatePropertyProvider(instance, property, attr),
                FieldInfo field                         => CreateFieldProvider(instance, field, attr),
                _                                       => null
            };
        }
        catch (Exception ex)
        {
            Error($"创建 Provider 失败: {attr.IPCName}", ex);
            return null;
        }
    }

    private static bool CreateSubscriber(object? instance, MemberInfo member, IPCSubscriberAttribute attr)
    {
        try
        {
            return member switch
            {
                FieldInfo field => CreateFieldSubscriber(instance, field, attr),
                _               => false
            };
        }
        catch (Exception ex)
        {
            Error($"创建 Subscriber 失败: {attr.IPCName}", ex);
            return false;
        }
    }

    private static ICallGateProvider? CreateMethodProvider(object? instance, MethodInfo method, IPCProviderAttribute attr)
    {
        var parameters = method.GetParameters();
        var returnType = method.ReturnType;

        if (returnType == typeof(void))
            return CreateActionProvider(instance, method, attr.IPCName, parameters);

        return CreateFuncProvider(instance, method, attr.IPCName, parameters, returnType);
    }

    private static ICallGateProvider? CreateActionProvider(object? instance, MethodInfo method, string ipcName, ParameterInfo[] parameters)
    {
        if (parameters.Length >= ActionTypes.Length - 1) return null;

        try
        {
            object?   provider       = null;
            Delegate? actionDelegate = null;

            if (parameters.Length == 0)
            {
                provider = DService.Instance().PI.GetIpcProvider<object>(ipcName);
                if (instance == null)
                    actionDelegate = (Action)(() => method.Invoke(null, null));
                else
                    actionDelegate = (Action)(() => method.Invoke(instance, null));
            }
            else if (parameters.Length == 1)
            {
                var paramType = parameters[0].ParameterType;
                var getProviderMethods = typeof(IDalamudPluginInterface).GetMethods()
                                                                        .Where(m => m.Name == "GetIpcProvider" && m.GetGenericArguments().Length == 2)
                                                                        .ToArray();
                var getProviderMethod = getProviderMethods.FirstOrDefault();
                if (getProviderMethod == null) return null;

                var genericMethod = getProviderMethod.MakeGenericMethod(paramType, typeof(object));
                provider = genericMethod.Invoke(DService.Instance().PI, [ipcName]);

                if (instance == null)
                {
                    actionDelegate = paramType switch
                    {
                        _ when paramType == typeof(bool)   => (Action<bool>)(p => method.Invoke(null,   [p])),
                        _ when paramType == typeof(string) => (Action<string>)(p => method.Invoke(null, [p])),
                        _ when paramType == typeof(float)  => (Action<float>)(p => method.Invoke(null,  [p])),
                        _ when paramType == typeof(int)    => (Action<int>)(p => method.Invoke(null,    [p])),
                        _                                  => null
                    };
                }
                else
                {
                    actionDelegate = paramType switch
                    {
                        _ when paramType == typeof(bool)   => (Action<bool>)(p => method.Invoke(instance,   [p])),
                        _ when paramType == typeof(string) => (Action<string>)(p => method.Invoke(instance, [p])),
                        _ when paramType == typeof(float)  => (Action<float>)(p => method.Invoke(instance,  [p])),
                        _ when paramType == typeof(int)    => (Action<int>)(p => method.Invoke(instance,    [p])),
                        _                                  => null
                    };
                }
            }
            else if (parameters.Length is >= 2 and <= 8)
            {
                var paramTypes        = parameters.Select(p => p.ParameterType).ToArray();
                var genericParamCount = paramTypes.Length + 1; // +1 for return type (object)

                var getProviderMethods = typeof(IDalamudPluginInterface).GetMethods()
                                                                        .Where
                                                                        (m => m.Name                         == "GetIpcProvider" &&
                                                                              m.GetGenericArguments().Length == genericParamCount
                                                                        )
                                                                        .ToArray();
                var getProviderMethod = getProviderMethods.FirstOrDefault();
                if (getProviderMethod == null) return null;

                var allGenericTypes = paramTypes.Append(typeof(object)).ToArray();
                var genericMethod   = getProviderMethod.MakeGenericMethod(allGenericTypes);
                provider = genericMethod.Invoke(DService.Instance().PI, [ipcName]);

                actionDelegate = CreateMultiParamAction(method, paramTypes, instance);
            }

            if (provider == null || actionDelegate == null) return null;

            var registerMethod = provider.GetType().GetMethod("RegisterAction");
            if (registerMethod == null) return null;

            registerMethod.Invoke(provider, [actionDelegate]);
            return (ICallGateProvider)provider;
        }
        catch (Exception ex)
        {
            Error($"创建 Action Provider 失败: {ipcName}", ex);
            return null;
        }
    }

    private static ICallGateProvider? CreateFuncProvider(object? instance, MethodInfo method, string ipcName, ParameterInfo[] parameters, Type returnType)
    {
        if (parameters.Length >= FuncTypes.Length) return null;

        try
        {
            var paramTypes        = parameters.Select(p => p.ParameterType).ToArray();
            var allGenericTypes   = paramTypes.Append(returnType).ToArray();
            var genericParamCount = allGenericTypes.Length;

            var getProviderMethods = typeof(IDalamudPluginInterface).GetMethods()
                                                                    .Where(m => m.Name == "GetIpcProvider" && m.GetGenericArguments().Length == genericParamCount)
                                                                    .ToArray();
            var getProviderMethod = getProviderMethods.FirstOrDefault();

            if (getProviderMethod == null)
            {
                Error($"创建 Func Provider 失败: {ipcName} - 找不到具有 {genericParamCount} 个泛型参数的 GetIpcProvider 方法");
                return null;
            }

            var genericMethod = getProviderMethod.MakeGenericMethod(allGenericTypes);
            var provider      = genericMethod.Invoke(DService.Instance().PI, [ipcName]);
            if (provider == null) return null;

            var funcDelegate = CreateMultiParamFunc(method, paramTypes, returnType, instance);
            if (funcDelegate == null) return null;

            var registerMethod = provider.GetType().GetMethod("RegisterFunc");
            registerMethod?.Invoke(provider, [funcDelegate]);

            return (ICallGateProvider)provider;
        }
        catch (Exception ex)
        {
            Error($"创建 Func Provider 失败: {ipcName}", ex);
            return null;
        }
    }

    private static Delegate? CreateMultiParamAction(MethodInfo method, Type[] paramTypes, object? instance)
    {
        try
        {
            var actionType = paramTypes.Length switch
            {
                2 => typeof(Action<,>).MakeGenericType(paramTypes),
                3 => typeof(Action<,,>).MakeGenericType(paramTypes),
                4 => typeof(Action<,,,>).MakeGenericType(paramTypes),
                5 => typeof(Action<,,,,>).MakeGenericType(paramTypes),
                6 => typeof(Action<,,,,,>).MakeGenericType(paramTypes),
                7 => typeof(Action<,,,,,,>).MakeGenericType(paramTypes),
                8 => typeof(Action<,,,,,,,>).MakeGenericType(paramTypes),
                _ => null
            };

            if (actionType == null) return null;

            return method.IsStatic ? method.CreateDelegate(actionType) : method.CreateDelegate(actionType, instance);
        }
        catch (Exception ex)
        {
            Error($"创建多参数 Action 委托失败: {method.Name}", ex);
            return null;
        }
    }

    private static Delegate? CreateMultiParamFunc(MethodInfo method, Type[] paramTypes, Type returnType, object? instance)
    {
        try
        {
            var allTypes = paramTypes.Append(returnType).ToArray();
            var funcType = allTypes.Length switch
            {
                1 => typeof(Func<>).MakeGenericType(allTypes),
                2 => typeof(Func<,>).MakeGenericType(allTypes),
                3 => typeof(Func<,,>).MakeGenericType(allTypes),
                4 => typeof(Func<,,,>).MakeGenericType(allTypes),
                5 => typeof(Func<,,,,>).MakeGenericType(allTypes),
                6 => typeof(Func<,,,,,>).MakeGenericType(allTypes),
                7 => typeof(Func<,,,,,,>).MakeGenericType(allTypes),
                8 => typeof(Func<,,,,,,,>).MakeGenericType(allTypes),
                9 => typeof(Func<,,,,,,,,>).MakeGenericType(allTypes),
                _ => null
            };

            if (funcType == null) return null;

            return method.IsStatic ? method.CreateDelegate(funcType) : method.CreateDelegate(funcType, instance);
        }
        catch (Exception ex)
        {
            Error($"创建多参数 Func 委托失败: {method.Name}", ex);
            return null;
        }
    }

    private static ICallGateProvider? CreatePropertyProvider(object? instance, PropertyInfo property, IPCProviderAttribute attr)
    {
        var propertyType = property.PropertyType;

        if (propertyType == typeof(bool))
        {
            var provider = DService.Instance().PI.GetIpcProvider<bool>(attr.IPCName);
            provider.RegisterFunc(() => (bool)property.GetValue(instance)!);
            return provider;
        }

        if (propertyType == typeof(string))
        {
            var provider = DService.Instance().PI.GetIpcProvider<string>(attr.IPCName);
            provider.RegisterFunc(() => (string)property.GetValue(instance)!);
            return provider;
        }

        if (propertyType == typeof(float))
        {
            var provider = DService.Instance().PI.GetIpcProvider<float>(attr.IPCName);
            provider.RegisterFunc(() => (float)property.GetValue(instance)!);
            return provider;
        }

        return null;
    }

    private static ICallGateProvider? CreateFieldProvider(object? instance, FieldInfo field, IPCProviderAttribute attr)
    {
        var fieldType = field.FieldType;

        if (fieldType == typeof(bool))
        {
            var provider = DService.Instance().PI.GetIpcProvider<bool>(attr.IPCName);
            provider.RegisterFunc(() => (bool)field.GetValue(instance)!);
            return provider;
        }

        return null;
    }

    private static bool CreateFieldSubscriber(object? instance, FieldInfo field, IPCSubscriberAttribute attr)
    {
        var fieldType = field.FieldType;

        if (fieldType.IsGenericType && fieldType.Name.StartsWith("IPCSubscriber"))
            return CreateCustomFieldSubscriber(instance, field, attr);

        return false;
    }

    private static bool CreateCustomFieldSubscriber(object? instance, FieldInfo field, IPCSubscriberAttribute attr)
    {
        var fieldType   = field.FieldType;
        var genericArgs = fieldType.GetGenericArguments();

        try
        {
            object? defaultValueFactory = null;

            if (genericArgs.Length == 1 && !string.IsNullOrEmpty(attr.DefaultValue))
            {
                try
                {
                    var valueType                         = genericArgs[0];
                    var getTypedDefaultValueMethod        = typeof(IPCSubscriberAttribute).GetMethod(nameof(IPCSubscriberAttribute.GetTypedDefaultValue));
                    var genericGetTypedDefaultValueMethod = getTypedDefaultValueMethod!.MakeGenericMethod(valueType);
                    var typedDefaultValue                 = genericGetTypedDefaultValueMethod.Invoke(attr, null);

                    if (typedDefaultValue != null)
                    {
                        var method        = typeof(IPCAttributeRegistry).GetMethod(nameof(CreateDefaultValueFunc), BindingFlags.NonPublic | BindingFlags.Static);
                        var genericMethod = method!.MakeGenericMethod(valueType);
                        defaultValueFactory = genericMethod.Invoke(null, [typedDefaultValue]);
                    }
                }
                catch (Exception ex)
                {
                    Error($"解析默认值失败: DefaultValue='{attr.DefaultValue}', IPCName='{attr.IPCName}'", ex);
                }
            }

            var subscriber = genericArgs.Length switch
            {
                1 => Activator.CreateInstance(typeof(IPCSubscriber<>).MakeGenericType(genericArgs),         attr.IPCName, defaultValueFactory),
                2 => Activator.CreateInstance(typeof(IPCSubscriber<,>).MakeGenericType(genericArgs),        attr.IPCName, defaultValueFactory),
                3 => Activator.CreateInstance(typeof(IPCSubscriber<,,>).MakeGenericType(genericArgs),       attr.IPCName, defaultValueFactory),
                4 => Activator.CreateInstance(typeof(IPCSubscriber<,,,>).MakeGenericType(genericArgs),      attr.IPCName, defaultValueFactory),
                5 => Activator.CreateInstance(typeof(IPCSubscriber<,,,,>).MakeGenericType(genericArgs),     attr.IPCName, defaultValueFactory),
                6 => Activator.CreateInstance(typeof(IPCSubscriber<,,,,,>).MakeGenericType(genericArgs),    attr.IPCName, defaultValueFactory),
                7 => Activator.CreateInstance(typeof(IPCSubscriber<,,,,,,>).MakeGenericType(genericArgs),   attr.IPCName, defaultValueFactory),
                8 => Activator.CreateInstance(typeof(IPCSubscriber<,,,,,,,>).MakeGenericType(genericArgs),  attr.IPCName, defaultValueFactory),
                9 => Activator.CreateInstance(typeof(IPCSubscriber<,,,,,,,,>).MakeGenericType(genericArgs), attr.IPCName, defaultValueFactory),
                _ => null
            };

            if (subscriber == null) return false;

            // 设置AutoInitialize属性
            var autoInitProperty = subscriber.GetType().GetProperty("AutoInitialize");
            autoInitProperty?.SetValue(subscriber, attr.AutoInitialize);

            // 设置字段值
            field.SetValue(instance, subscriber);

            // 如果AutoInitialize为true，尝试立即初始化
            if (attr.AutoInitialize)
            {
                var initMethod = subscriber.GetType().GetMethod("Initialize");
                initMethod?.Invoke(subscriber, null);
            }

            return true;
        }
        catch (Exception ex)
        {
            Error($"创建自定义字段 Subscriber 失败: {attr.IPCName}", ex);
            return false;
        }
    }

    private static Func<T> CreateDefaultValueFunc<T>(object defaultValue)
    {
        return () =>
        {
            try
            {
                // 如果默认值已经是目标类型，直接返回
                if (defaultValue is T directValue)
                    return directValue;

                // 如果默认值为null，返回类型默认值
                if (defaultValue == null)
                    return default!;

                // 尝试安全的类型转换
                var targetType = typeof(T);

                // 处理可空类型
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var underlyingType = Nullable.GetUnderlyingType(targetType)!;
                    var convertedValue = Convert.ChangeType(defaultValue, underlyingType);
                    return (T)convertedValue;
                }

                // 处理基本类型转换
                if (targetType.IsPrimitive || targetType == typeof(string) || targetType == typeof(decimal))
                    return (T)Convert.ChangeType(defaultValue, targetType);

                // 最后尝试直接转换
                return (T)defaultValue;
            }
            catch (Exception ex)
            {
                Error($"类型转换失败: 无法将 {defaultValue?.GetType().Name ?? "null"} 转换为 {typeof(T).Name}", ex);
                return default!;
            }
        };
    }
}
