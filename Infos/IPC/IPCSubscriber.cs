using Dalamud.Plugin.Ipc;

namespace OmenTools.Infos;

/// <summary>
/// IPC 订阅者的抽象基类，封装了通用逻辑
/// </summary>
/// <typeparam name="TSubscriber">IPC 订阅者接口类型</typeparam>
public abstract class IPCSubscriberBase<TSubscriber>(string ipcName) : IDisposable
    where TSubscriber : class
{
    protected readonly string ipcName  = ipcName ?? throw new ArgumentNullException(nameof(ipcName));
    protected readonly Lock   initLock = new();
    protected volatile bool   isInitialized;
    protected          bool   disposed;

    protected TSubscriber? subscriber;

    /// <summary>
    /// 获取或设置是否在首次调用时自动初始化
    /// </summary>
    public bool AutoInitialize { get; set; } = true;

    /// <summary>
    /// 检查IPC是否可用
    /// </summary>
    public bool IsAvailable => isInitialized && subscriber != null;

    /// <summary>
    /// 创建具体的 IPC 订阅者实例
    /// </summary>
    /// <returns>订阅者实例</returns>
    protected abstract TSubscriber CreateSubscriber();

    /// <summary>
    /// 手动初始化IPC连接
    /// </summary>
    /// <returns>初始化是否成功</returns>
    public bool Initialize()
    {
        if (disposed) return false;
        lock (initLock)
        {
            if (isInitialized) return true;

            try
            {
                subscriber = CreateSubscriber();
                isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                Error($"初始化IPC订阅者 {ipcName} 失败", ex);
                subscriber = null;
                return false;
            }
        }
    }

    /// <summary>
    /// 确保订阅者已初始化
    /// </summary>
    protected void EnsureInitialized()
    {
        if (!disposed && !isInitialized && AutoInitialize)
            Initialize();
    }

    /// <summary>
    /// 重置初始化状态，允许重新初始化
    /// </summary>
    public void Reset()
    {
        lock (initLock)
        {
            isInitialized = false;
            subscriber = null;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        Reset();
        GC.SuppressFinalize(this);
    }
}


/// <summary>
///     IPC订阅者包装类，提供类似原生类型的使用体验
///     支持延迟初始化、自动重试和直接类型转换
/// </summary>
/// <typeparam name="T">IPC返回的数据类型</typeparam>
public class IPCSubscriber<T> : IPCSubscriberBase<ICallGateSubscriber<T>>
{
    private readonly Func<T> DefaultValueFactory;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="ipcName">IPC名称</param>
    /// <param name="defaultValueFactory">默认值工厂函数</param>
    public IPCSubscriber(string ipcName, Func<T>? defaultValueFactory = null) : base(ipcName) => 
        DefaultValueFactory = defaultValueFactory ?? (() => default!);

    protected override ICallGateSubscriber<T> CreateSubscriber() => DService.PI.GetIpcSubscriber<T>(ipcName);

    /// <summary>
    ///     获取IPC值，如果未初始化则返回默认值
    /// </summary>
    public T Value
    {
        get
        {
            EnsureInitialized();

            if (subscriber != null)
            {
                try { return subscriber.InvokeFunc(); }
                catch (Exception ex)
                {
                    Error($"调用IPC {ipcName} 时发生错误", ex);
                    return DefaultValueFactory();
                }
            }

            return DefaultValueFactory();
        }
    }

    /// <summary>
    ///     显式调用IPC函数
    /// </summary>
    public T InvokeFunc()
    {
        EnsureInitialized();

        if (subscriber != null)
        {
            try { return subscriber.InvokeFunc(); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
                return DefaultValueFactory();
            }
        }

        return DefaultValueFactory();
    }

    /// <summary>
    ///     显式调用IPC Action
    /// </summary>
    public void InvokeAction()
    {
        EnsureInitialized();

        if (subscriber != null)
        {
            try { subscriber.InvokeAction(); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
            }
        }
    }

    #region 隐式转换操作符

    /// <summary>
    ///     隐式转换到T类型
    /// </summary>
    public static implicit operator T(IPCSubscriber<T> subscriber)
    {
        if (subscriber == null) return default!;
        return subscriber.Value;
    }

    #endregion

    #region 运算符重载

    // 相等性比较
    public static bool operator ==(IPCSubscriber<T> left, T right)
    {
        if (left is null) return right is null || (right != null && right.Equals(default(T)));
        return left.Value?.Equals(right) ?? right is null;
    }

    public static bool operator !=(IPCSubscriber<T> left, T right) =>
        !(left == right);

    public static bool operator ==(T left, IPCSubscriber<T> right) =>
        right == left;

    public static bool operator !=(T left, IPCSubscriber<T> right) =>
        !(right == left);

    public static bool operator ==(IPCSubscriber<T> left, IPCSubscriber<T> right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Value?.Equals(right.Value) ?? right.Value is null;
    }

    public static bool operator !=(IPCSubscriber<T> left, IPCSubscriber<T> right) =>
        !(left == right);

    #endregion

    #region 数值运算符重载（仅适用于数值类型）

    public static dynamic operator +(IPCSubscriber<T> left, T right)
    {
        if (left == null) return right;
        return (dynamic)left.Value + (dynamic)right;
    }

    public static dynamic operator +(T left, IPCSubscriber<T> right)
    {
        if (right == null) return left;
        return (dynamic)left + (dynamic)right.Value;
    }

    public static dynamic operator -(IPCSubscriber<T> left, T right)
    {
        if (left == null) return -(dynamic)right;
        return (dynamic)left.Value - (dynamic)right;
    }

    public static dynamic operator -(T left, IPCSubscriber<T> right)
    {
        if (right == null) return left;
        return (dynamic)left - (dynamic)right.Value;
    }

    public static dynamic operator *(IPCSubscriber<T> left, T right)
    {
        if (left == null) return default(T);
        return (dynamic)left.Value * (dynamic)right;
    }

    public static dynamic operator *(T left, IPCSubscriber<T> right)
    {
        if (right == null) return default(T);
        return (dynamic)left * (dynamic)right.Value;
    }

    public static dynamic operator /(IPCSubscriber<T> left, T right)
    {
        if (left == null) return default(T);
        return (dynamic)left.Value / (dynamic)right;
    }

    public static dynamic operator /(T left, IPCSubscriber<T> right)
    {
        if (right == null)
            throw new DivideByZeroException();
        return (dynamic)left / (dynamic)right.Value;
    }

    #endregion

    #region 比较运算符重载（适用于IComparable类型）

    // 注意：运算符重载不能有泛型约束，所以需要在运行时检查类型
    public static bool operator <(IPCSubscriber<T> left, T right)
    {
        if (left == null) return right != null;
        if (left.Value is IComparable<T> comparable)
            return comparable.CompareTo(right) < 0;
        return false;
    }

    public static bool operator >(IPCSubscriber<T> left, T right)
    {
        if (left == null) return false;
        if (left.Value is IComparable<T> comparable)
            return comparable.CompareTo(right) > 0;
        return false;
    }

    public static bool operator <=(IPCSubscriber<T> left, T right) =>
        left < right || left == right;

    public static bool operator >=(IPCSubscriber<T> left, T right) =>
        left > right || left == right;

    #endregion

    #region Object重写

    public override bool Equals(object? obj) => obj switch
    {
        IPCSubscriber<T> other => this == other,
        T value                => this == value,
        _                      => false
    };

    public override int GetHashCode() =>
        Value?.GetHashCode() ?? 0;

    public override string ToString() =>
        Value?.ToString() ?? "null";

    #endregion
}

/// <summary>
///     支持单参数调用的IPC订阅者包装类
/// </summary>
public class IPCSubscriber<T1, TResult>(string ipcName, Func<TResult>? defaultValueFactory = null) : IPCSubscriberBase<ICallGateSubscriber<T1, TResult>>(ipcName)
{
    private readonly Func<TResult> defaultValueFactory = defaultValueFactory ?? (() => default!);

    protected override ICallGateSubscriber<T1, TResult> CreateSubscriber() => DService.PI.GetIpcSubscriber<T1, TResult>(ipcName);

    public TResult InvokeFunc(T1 arg1)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { return subscriber.InvokeFunc(arg1); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
                return defaultValueFactory();
            }
        }
        return defaultValueFactory();
    }

    public void InvokeAction(T1 arg1)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { subscriber.InvokeAction(arg1); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
            }
        }
    }
}

/// <summary>
///     支持双参数调用的IPC订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, TResult>(string ipcName, Func<TResult>? defaultValueFactory = null)
    : IPCSubscriberBase<ICallGateSubscriber<T1, T2, TResult>>(ipcName)
{
    private readonly Func<TResult> defaultValueFactory = defaultValueFactory ?? (() => default!);

    protected override ICallGateSubscriber<T1, T2, TResult> CreateSubscriber() => DService.PI.GetIpcSubscriber<T1, T2, TResult>(ipcName);

    public TResult InvokeFunc(T1 arg1, T2 arg2)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { return subscriber.InvokeFunc(arg1, arg2); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
                return defaultValueFactory();
            }
        }
        return defaultValueFactory();
    }

    public void InvokeAction(T1 arg1, T2 arg2)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { subscriber.InvokeAction(arg1, arg2); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
            }
        }
    }
}

/// <summary>
///     支持三参数调用的IPC订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, TResult>(string ipcName, Func<TResult>? defaultValueFactory = null)
    : IPCSubscriberBase<ICallGateSubscriber<T1, T2, T3, TResult>>(ipcName)
{
    private readonly Func<TResult> defaultValueFactory = defaultValueFactory ?? (() => default!);

    protected override ICallGateSubscriber<T1, T2, T3, TResult> CreateSubscriber() => DService.PI.GetIpcSubscriber<T1, T2, T3, TResult>(ipcName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { return subscriber.InvokeFunc(arg1, arg2, arg3); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
                return defaultValueFactory();
            }
        }
        return defaultValueFactory();
    }

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { subscriber.InvokeAction(arg1, arg2, arg3); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
            }
        }
    }
}

/// <summary>
///     支持四参数调用的IPC订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, T4, TResult>(string ipcName, Func<TResult>? defaultValueFactory = null)
    : IPCSubscriberBase<ICallGateSubscriber<T1, T2, T3, T4, TResult>>(ipcName)
{
    private readonly Func<TResult> defaultValueFactory = defaultValueFactory ?? (() => default!);

    protected override ICallGateSubscriber<T1, T2, T3, T4, TResult> CreateSubscriber() => DService.PI.GetIpcSubscriber<T1, T2, T3, T4, TResult>(ipcName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { return subscriber.InvokeFunc(arg1, arg2, arg3, arg4); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
                return defaultValueFactory();
            }
        }
        return defaultValueFactory();
    }

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { subscriber.InvokeAction(arg1, arg2, arg3, arg4); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
            }
        }
    }
}

/// <summary>
///     支持五参数调用的IPC订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, T4, T5, TResult>(string ipcName, Func<TResult>? defaultValueFactory = null)
    : IPCSubscriberBase<ICallGateSubscriber<T1, T2, T3, T4, T5, TResult>>(ipcName)
{
    private readonly Func<TResult> defaultValueFactory = defaultValueFactory ?? (() => default!);

    protected override ICallGateSubscriber<T1, T2, T3, T4, T5, TResult> CreateSubscriber() => DService.PI.GetIpcSubscriber<T1, T2, T3, T4, T5, TResult>(ipcName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { return subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
                return defaultValueFactory();
            }
        }
        return defaultValueFactory();
    }

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
            }
        }
    }
}

/// <summary>
///     支持六参数调用的IPC订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, T4, T5, T6, TResult>(string ipcName, Func<TResult>? defaultValueFactory = null)
    : IPCSubscriberBase<ICallGateSubscriber<T1, T2, T3, T4, T5, T6, TResult>>(ipcName)
{
    private readonly Func<TResult> defaultValueFactory = defaultValueFactory ?? (() => default!);

    protected override ICallGateSubscriber<T1, T2, T3, T4, T5, T6, TResult> CreateSubscriber() => DService.PI.GetIpcSubscriber<T1, T2, T3, T4, T5, T6, TResult>(ipcName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { return subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5, arg6); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
                return defaultValueFactory();
            }
        }
        return defaultValueFactory();
    }

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5, arg6); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
            }
        }
    }
}

/// <summary>
///     支持七参数调用的IPC订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, T4, T5, T6, T7, TResult>(string ipcName, Func<TResult>? defaultValueFactory = null)
    : IPCSubscriberBase<ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, TResult>>(ipcName)
{
    private readonly Func<TResult> defaultValueFactory = defaultValueFactory ?? (() => default!);

    protected override ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, TResult> CreateSubscriber() => DService.PI.GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, TResult>(ipcName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { return subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5, arg6, arg7); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
                return defaultValueFactory();
            }
        }
        return defaultValueFactory();
    }

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5, arg6, arg7); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
            }
        }
    }
}

/// <summary>
///     支持八参数调用的IPC订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string ipcName, Func<TResult>? defaultValueFactory = null)
    : IPCSubscriberBase<ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>(ipcName)
{
    private readonly Func<TResult> defaultValueFactory = defaultValueFactory ?? (() => default!);

    protected override ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TResult> CreateSubscriber() => DService.PI.GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(ipcName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { return subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
                return defaultValueFactory();
            }
        }
        return defaultValueFactory();
    }

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        EnsureInitialized();
        if (subscriber != null)
        {
            try { subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8); }
            catch (Exception ex)
            {
                Error($"调用IPC {ipcName} 时发生错误", ex);
            }
        }
    }
}
