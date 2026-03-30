using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Dalamud.Interface.Windowing;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public sealed class WindowManager : OmenServiceBase<WindowManager>
{
    public WindowSystem WindowSystem
    {
        get
        {
            if (field != null) return field;

            field = new(DService.Instance().PI.InternalName);

            DService.Instance().UIBuilder.Draw += OnWindowManagerDraw;

            return field;
        }
    }

    public ConcurrentDictionary<Type, Window> UniqueWindows { get; } = [];
    
    /// <summary>
    ///     在窗口系统绘制前调用
    /// </summary>
    public event Action? PreDraw;

    /// <summary>
    ///     在窗口系统绘制后调用
    /// </summary>
    public event Action? PostDraw;

    private ImmutableArray<DrawScopeRegistration> drawScopeRegistrations = [];

    private int nextDrawScopeRegistrationID;

    protected override void Uninit()
    {
        DService.Instance().UIBuilder.Draw -= OnWindowManagerDraw;
        PreDraw                            =  null;
        PostDraw                           =  null;
        drawScopeRegistrations             =  [];

        WindowSystem.RemoveAllWindows();

        foreach (var window in UniqueWindows.Values)
        {
            if (window is IDisposable disposableWindow)
                disposableWindow.Dispose();
        }

        UniqueWindows.Clear();
    }

    private void OnWindowManagerDraw()
    {
        var regs = this.drawScopeRegistrations;
        IDisposable?[]? activeScopes = null;

        try
        {
            PreDraw?.Invoke();

            if (!regs.IsDefaultOrEmpty)
            {
                activeScopes = ArrayPool<IDisposable?>.Shared.Rent(regs.Length);
                Array.Clear(activeScopes, 0, regs.Length);

                for (var i = 0; i < regs.Length; i++)
                    activeScopes[i] = regs[i].Factory();
            }

            WindowSystem.Draw();
            PostDraw?.Invoke();
        }
        finally
        {
            if (activeScopes != null)
            {
                for (var i = regs.Length - 1; i >= 0; i--)
                {
                    activeScopes[i]?.Dispose();
                    activeScopes[i] = null;
                }

                ArrayPool<IDisposable?>.Shared.Return(activeScopes);
            }
        }
    }

    /// <summary>
    ///     注册一组在整个窗口系统绘制期间保持生效的作用域
    ///     一般用于推入界面样式、字体设置
    /// </summary>
    public DrawScopesHandle RegDrawScopes(params Func<IDisposable?>[] factories)
    {
        ArgumentNullException.ThrowIfNull(factories);

        if (factories.Length == 0)
            return default;

        foreach (var t in factories)
            ArgumentNullException.ThrowIfNull(t);

        var handle = new DrawScopesHandle(Interlocked.Increment(ref nextDrawScopeRegistrationID));

        foreach (var t in factories)
        {
            var registration = new DrawScopeRegistration
            (
                handle.ID,
                t
            );

            ImmutableInterlocked.Update
            (
                ref drawScopeRegistrations,
                static (registrations, item) => registrations.Add(item),
                registration
            );
        }

        return handle;
    }

    /// <summary>
    ///     取消注册一组绘制作用域
    /// </summary>
    public void UnregDrawScopes(DrawScopesHandle handle)
    {
        if (!handle.IsValid)
            return;

        ImmutableInterlocked.Update
        (
            ref drawScopeRegistrations,
            static (registrations, targetID) => registrations.RemoveAll(registration => registration.TokenID == targetID),
            handle.ID
        );
    }

    /// <summary>
    ///     用于添加一个唯一的窗口, 存放至 <see cref="UniqueWindows" />
    ///     如果是可复用的窗口, 请使用 <see cref="AddWindow" />
    ///     在强制添加时, 如果窗口实现了 <see cref="IDisposable" /> 接口, 则会自动为旧窗口调用 Dispose 方法
    /// </summary>
    public bool AddWindow<T>(bool isForceToAdd = false) where T : Window
    {
        if (isForceToAdd)
        {
            if (UniqueWindows.TryGetValue(typeof(T), out var window))
            {
                WindowSystem.RemoveWindow(window);
                if (window is IDisposable disposableWindow)
                    disposableWindow.Dispose();
            }

            if (Activator.CreateInstance<T>() is not Window newWindow)
                return false;

            WindowSystem.AddWindow(newWindow);
            UniqueWindows[typeof(T)] = newWindow;
        }
        else
        {
            if (UniqueWindows.TryGetValue(typeof(T), out _))
                return false;

            if (Activator.CreateInstance<T>() is not Window newWindow)
                return false;

            WindowSystem.AddWindow(newWindow);
            UniqueWindows[typeof(T)] = newWindow;
        }

        return true;
    }

    /// <summary>
    ///     用于移除一个唯一的窗口, 从 <see cref="UniqueWindows" /> 取出移除
    ///     如果是可复用的窗口, 请使用非泛型 <see cref="RemoveWindow" />
    ///     如果窗口实现了 <see cref="IDisposable" /> 接口, 则会自动调用 Dispose 方法
    /// </summary>
    public bool RemoveWindow<T>() where T : Window
    {
        if (!UniqueWindows.TryRemove(typeof(T), out var window))
            return false;

        if (window is IDisposable disposableWindow)
            disposableWindow.Dispose();

        return true;
    }

    /// <summary>
    ///     用于移除一个可复用的窗口, 请自行存储实例
    ///     如果是唯一的窗口, 请使用泛型 <see cref="RemoveWindow" />
    ///     如果窗口实现了 <see cref="IDisposable" /> 接口, 则会自动调用 Dispose 方法
    /// </summary>
    public bool AddWindow(Window window, bool isForceToAdd = false)
    {
        ArgumentNullException.ThrowIfNull(window);

        var addedWindows = WindowSystem.Windows;

        if (isForceToAdd)
        {
            if (addedWindows.Contains(window))
            {
                WindowSystem.RemoveWindow(window);
                if (window is IDisposable disposableWindow)
                    disposableWindow.Dispose();
            }

        }
        else
        {
            if (addedWindows.Contains(window))
                return false;
        }

        WindowSystem.AddWindow(window);
        return true;
    }

    /// <summary>
    ///     用于移除一个可复用的窗口
    ///     如果是唯一的窗口, 请使用泛型 <see cref="RemoveWindow" />
    ///     如果窗口实现了 <see cref="IDisposable" /> 接口, 则会自动调用 Dispose 方法
    /// </summary>
    public bool RemoveWindow(Window? window)
    {
        if (window == null) return false;

        var addedWindows = WindowSystem.Windows;
        if (!addedWindows.Contains(window)) return false;

        WindowSystem.RemoveWindow(window);
        if (window is IDisposable disposableWindow)
            disposableWindow.Dispose();

        return true;
    }

    /// <summary>
    ///     用于获取一个唯一的窗口, 从 <see cref="UniqueWindows" /> 查询
    /// </summary>
    public T? Get<T>() where T : Window =>
        UniqueWindows.TryGetValue(typeof(T), out var window) ? window as T : null;

    private readonly record struct DrawScopeRegistration
    (
        int                TokenID,
        Func<IDisposable?> Factory
    );
}
