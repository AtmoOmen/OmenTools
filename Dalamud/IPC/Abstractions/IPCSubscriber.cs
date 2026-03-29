using System.Collections.Concurrent;
using Dalamud.Plugin.Ipc;

namespace OmenTools.Dalamud.Abstractions;

internal static class IPCErrorNotificationRegistry
{
    private static readonly ConcurrentDictionary<string, byte> NotifiedErrorIpcNames = [];

    public static bool TryMark(string ipcName) => NotifiedErrorIpcNames.TryAdd(ipcName, 0);

    public static void Clear(string ipcName) => NotifiedErrorIpcNames.TryRemove(ipcName, out _);
}

/// <summary>
///     IPC 订阅者的非泛型基类，负责生命周期和初始化流程
/// </summary>
public abstract class IPCSubscriberBase
(
    string ipcName
) : IDisposable
{
    protected readonly string IPCName  = ipcName ?? throw new ArgumentNullException(nameof(ipcName));
    protected readonly Lock   InitLock = new();
    protected volatile bool   IsInitialized;
    protected          bool   IsDisposed;

    /// <summary>
    ///     获取或设置是否在首次调用时自动初始化
    /// </summary>
    public bool AutoInitialize { get; set; } = true;

    /// <summary>
    ///     当前订阅者是否可用
    /// </summary>
    public abstract bool IsAvailable { get; }

    /// <summary>
    ///     手动初始化 IPC 连接
    /// </summary>
    public bool Initialize()
    {
        if (IsDisposed) return false;

        lock (InitLock)
        {
            if (IsInitialized) return true;

            try
            {
                CreateSubscriberCore();
                IsInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                ResetSubscriberCore();
                DLog.Error($"初始化 IPC 订阅者 {IPCName} 失败", ex);
                return false;
            }
        }
    }

    /// <summary>
    ///     重置初始化状态，允许重新初始化
    /// </summary>
    public void Reset()
    {
        lock (InitLock)
        {
            IsInitialized = false;
            ResetSubscriberCore();
        }
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;

        IsDisposed = true;
        Reset();
        GC.SuppressFinalize(this);
    }

    protected void EnsureInitialized()
    {
        if (!IsDisposed && !IsInitialized && AutoInitialize)
            Initialize();
    }

    protected abstract void CreateSubscriberCore();

    protected abstract void ResetSubscriberCore();
}

/// <summary>
///     IPC 订阅者的泛型基类，封装了统一的调用逻辑
/// </summary>
/// <typeparam name="TSubscriber">实际的 Dalamud IPC 订阅者类型</typeparam>
public abstract class IPCSubscriberBase<TSubscriber>
(
    string ipcName
) : IPCSubscriberBase(ipcName)
    where TSubscriber : class
{
    protected TSubscriber? Subscriber;

    public override bool IsAvailable => IsInitialized && Subscriber != null;

    protected sealed override void CreateSubscriberCore() => Subscriber = CreateSubscriber();

    protected sealed override void ResetSubscriberCore() => Subscriber = null;

    protected abstract TSubscriber CreateSubscriber();

    protected TResult InvokeCore<TResult>
        (Func<TSubscriber, TResult> operation, Func<TResult> fallback, bool suppressException = false, bool deduplicateError = false)
    {
        EnsureInitialized();

        if (Subscriber is not { } subscriber)
            return fallback();

        try
        {
            var result = operation(subscriber);
            IPCErrorNotificationRegistry.Clear(IPCName);
            return result;
        }
        catch (Exception ex)
        {
            if (!suppressException && (!deduplicateError || IPCErrorNotificationRegistry.TryMark(IPCName)))
                DLog.Error($"调用 IPC {IPCName} 时发生错误", ex);

            return fallback();
        }
    }

    protected void InvokeCore(Action<TSubscriber> operation, bool suppressException = false)
    {
        EnsureInitialized();

        if (Subscriber is not { } subscriber)
            return;

        try
        {
            operation(subscriber);
            IPCErrorNotificationRegistry.Clear(IPCName);
        }
        catch (Exception ex)
        {
            if (!suppressException)
                DLog.Error($"调用 IPC {IPCName} 时发生错误", ex);
        }
    }
}

/// <summary>
///     带返回值的 IPC 订阅者基类，统一默认值和失败回退逻辑
/// </summary>
public abstract class IPCResultSubscriberBase<TSubscriber, TResult>
(
    string         ipcName,
    Func<TResult>? defaultValueFactory = null
) : IPCSubscriberBase<TSubscriber>(ipcName)
    where TSubscriber : class
{
    private readonly Func<TResult> defaultValueFactory = defaultValueFactory ?? (() => default!);

    protected TResult InvokeValue(Func<TSubscriber, TResult> operation, bool suppressException = false, bool deduplicateError = false) =>
        InvokeCore(operation, defaultValueFactory, suppressException, deduplicateError);
}

/// <summary>
///     无参数 IPC 订阅者包装类
/// </summary>
public class IPCSubscriber<T>
(
    string   ipcName,
    Func<T>? defaultValueFactory = null
) : IPCResultSubscriberBase<ICallGateSubscriber<T>, T>(ipcName, defaultValueFactory)
{
    protected override ICallGateSubscriber<T> CreateSubscriber() => DService.Instance().PI.GetIpcSubscriber<T>(IPCName);

    /// <summary>
    ///     获取 IPC 值。如果调用失败，则返回默认值
    /// </summary>
    public T Value => InvokeValue(static subscriber => subscriber.InvokeFunc(), deduplicateError: true);

    public T InvokeFunc() => InvokeValue(static subscriber => subscriber.InvokeFunc());

    public void InvokeAction() => InvokeCore(static subscriber => subscriber.InvokeAction());

    public T TryInvokeFunc() => InvokeValue(static subscriber => subscriber.InvokeFunc(), true);

    public void TryInvokeAction() => InvokeCore(static subscriber => subscriber.InvokeAction(), true);

    public static implicit operator T(IPCSubscriber<T>? subscriber) => subscriber is null ? default! : subscriber.Value;

    public override bool Equals(object? obj)
    {
        var comparer = EqualityComparer<T>.Default;

        return obj switch
        {
            IPCSubscriber<T> other => comparer.Equals(Value, other.Value),
            T value                => comparer.Equals(Value, value),
            _                      => false
        };
    }

    public override int GetHashCode()
    {
        var value = Value;
        return value is null ? 0 : EqualityComparer<T>.Default.GetHashCode(value);
    }

    public override string ToString()
    {
        var value = Value;
        return value?.ToString() ?? "null";
    }
}

/// <summary>
///     支持单参数调用的 IPC 订阅者包装类
/// </summary>
public class IPCSubscriber<T1, TResult>
(
    string         ipcName,
    Func<TResult>? defaultValueFactory = null
) : IPCResultSubscriberBase<ICallGateSubscriber<T1, TResult>, TResult>(ipcName, defaultValueFactory)
{
    protected override ICallGateSubscriber<T1, TResult> CreateSubscriber() => DService.Instance().PI.GetIpcSubscriber<T1, TResult>(IPCName);

    public TResult InvokeFunc(T1 arg1) => InvokeValue(subscriber => subscriber.InvokeFunc(arg1), deduplicateError: true);

    public void InvokeAction(T1 arg1) => InvokeCore(subscriber => subscriber.InvokeAction(arg1));

    public TResult TryInvokeFunc(T1 arg1) => InvokeValue(subscriber => subscriber.InvokeFunc(arg1), true);

    public void TryInvokeAction(T1 arg1) => InvokeCore(subscriber => subscriber.InvokeAction(arg1), true);
}

/// <summary>
///     支持双参数调用的 IPC 订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, TResult>
(
    string         ipcName,
    Func<TResult>? defaultValueFactory = null
) : IPCResultSubscriberBase<ICallGateSubscriber<T1, T2, TResult>, TResult>(ipcName, defaultValueFactory)
{
    protected override ICallGateSubscriber<T1, T2, TResult> CreateSubscriber() => DService.Instance().PI.GetIpcSubscriber<T1, T2, TResult>(IPCName);

    public TResult InvokeFunc(T1 arg1, T2 arg2) => InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2), deduplicateError: true);

    public void InvokeAction(T1 arg1, T2 arg2) => InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2));

    public TResult TryInvokeFunc(T1 arg1, T2 arg2) => InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2), true);

    public void TryInvokeAction(T1 arg1, T2 arg2) => InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2), true);
}

/// <summary>
///     支持三参数调用的 IPC 订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, TResult>
(
    string         ipcName,
    Func<TResult>? defaultValueFactory = null
) : IPCResultSubscriberBase<ICallGateSubscriber<T1, T2, T3, TResult>, TResult>(ipcName, defaultValueFactory)
{
    protected override ICallGateSubscriber<T1, T2, T3, TResult> CreateSubscriber() => DService.Instance().PI.GetIpcSubscriber<T1, T2, T3, TResult>(IPCName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3) => InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3), deduplicateError: true);

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3) => InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3));

    public TResult TryInvokeFunc(T1 arg1, T2 arg2, T3 arg3) => InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3), true);

    public void TryInvokeAction(T1 arg1, T2 arg2, T3 arg3) => InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3), true);
}

/// <summary>
///     支持四参数调用的 IPC 订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, T4, TResult>
(
    string         ipcName,
    Func<TResult>? defaultValueFactory = null
) : IPCResultSubscriberBase<ICallGateSubscriber<T1, T2, T3, T4, TResult>, TResult>(ipcName, defaultValueFactory)
{
    protected override ICallGateSubscriber<T1, T2, T3, T4, TResult> CreateSubscriber() => DService.Instance().PI.GetIpcSubscriber<T1, T2, T3, T4, TResult>(IPCName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => InvokeValue
        (subscriber => subscriber.InvokeFunc(arg1, arg2, arg3, arg4), deduplicateError: true);

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3, arg4));

    public TResult TryInvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3, arg4), true);

    public void TryInvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3, arg4), true);
}

/// <summary>
///     支持五参数调用的 IPC 订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, T4, T5, TResult>
(
    string         ipcName,
    Func<TResult>? defaultValueFactory = null
) : IPCResultSubscriberBase<ICallGateSubscriber<T1, T2, T3, T4, T5, TResult>, TResult>(ipcName, defaultValueFactory)
{
    protected override ICallGateSubscriber<T1, T2, T3, T4, T5, TResult> CreateSubscriber() =>
        DService.Instance().PI.GetIpcSubscriber<T1, T2, T3, T4, T5, TResult>(IPCName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) =>
        InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5), deduplicateError: true);

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) =>
        InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5));

    public TResult TryInvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) =>
        InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5), true);

    public void TryInvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) =>
        InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5), true);
}

/// <summary>
///     支持六参数调用的 IPC 订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, T4, T5, T6, TResult>
(
    string         ipcName,
    Func<TResult>? defaultValueFactory = null
) : IPCResultSubscriberBase<ICallGateSubscriber<T1, T2, T3, T4, T5, T6, TResult>, TResult>(ipcName, defaultValueFactory)
{
    protected override ICallGateSubscriber<T1, T2, T3, T4, T5, T6, TResult> CreateSubscriber() =>
        DService.Instance().PI.GetIpcSubscriber<T1, T2, T3, T4, T5, T6, TResult>(IPCName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) =>
        InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5, arg6), deduplicateError: true);

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) =>
        InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5, arg6));

    public TResult TryInvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) =>
        InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5, arg6), true);

    public void TryInvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) =>
        InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5, arg6), true);
}

/// <summary>
///     支持七参数调用的 IPC 订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, T4, T5, T6, T7, TResult>
(
    string         ipcName,
    Func<TResult>? defaultValueFactory = null
) : IPCResultSubscriberBase<ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, TResult>, TResult>(ipcName, defaultValueFactory)
{
    protected override ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, TResult> CreateSubscriber() =>
        DService.Instance().PI.GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, TResult>(IPCName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) =>
        InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5, arg6, arg7), deduplicateError: true);

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) =>
        InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5, arg6, arg7));

    public TResult TryInvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) =>
        InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5, arg6, arg7), true);

    public void TryInvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) =>
        InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5, arg6, arg7), true);
}

/// <summary>
///     支持八参数调用的 IPC 订阅者包装类
/// </summary>
public class IPCSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TResult>
(
    string         ipcName,
    Func<TResult>? defaultValueFactory = null
) : IPCResultSubscriberBase<ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TResult>, TResult>(ipcName, defaultValueFactory)
{
    protected override ICallGateSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TResult> CreateSubscriber() =>
        DService.Instance().PI.GetIpcSubscriber<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(IPCName);

    public TResult InvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) =>
        InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), deduplicateError: true);

    public void InvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) =>
        InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));

    public TResult TryInvokeFunc(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) =>
        InvokeValue(subscriber => subscriber.InvokeFunc(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), true);

    public void TryInvokeAction(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) =>
        InvokeCore(subscriber => subscriber.InvokeAction(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), true);
}
