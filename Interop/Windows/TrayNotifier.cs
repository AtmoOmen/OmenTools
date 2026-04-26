using System.Globalization;
using System.Threading.Channels;
using OmenTools.OmenService;
using Task = System.Threading.Tasks.Task;

namespace OmenTools.Interop.Windows;

public sealed class TrayNotifier : IDisposable
{
    /// <summary>
    ///     当短时间内收到多条消息时显示的聚合消息模板。
    ///     <example>收到了 {0} 条新消息</example>
    /// </summary>
    public string MultiMessageTemplate { get; set; } = "收到了 {0} 条新消息";

    /// <summary>
    ///     是否仅当窗口处于后台时才显示消息。
    /// </summary>
    public bool OnlyBackground { get; set; }

    /// <summary>
    ///     托盘图标。
    /// </summary>
    public required Icon Icon
    {
        get => Volatile.Read(ref icon) ?? throw new InvalidOperationException("托盘图标尚未初始化。");
        set
        {
            if (IsDisposed)
                return;

            ArgumentNullException.ThrowIfNull(value);

            Volatile.Write(ref icon, value);
            trayIconThread.SetIcon(value);
        }
    }

    private bool IsDisposed => Volatile.Read(ref isDisposed) != 0;

    private static readonly TimeSpan MergeWindow   = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan DisplayWindow = TimeSpan.FromSeconds(7);

    private readonly Channel<BalloonTipMessage> messageChannel = Channel.CreateUnbounded<BalloonTipMessage>
    (
        new()
        {
            SingleReader                  = true,
            SingleWriter                  = false,
            AllowSynchronousContinuations = false
        }
    );

    private readonly CancellationTokenSource disposalTokenSource = new();
    private readonly Task                    processingTask;
    private readonly TrayIconThread          trayIconThread;

    private int   isDisposed;
    private Icon? icon;

    public TrayNotifier()
    {
        trayIconThread = new();
        processingTask = Task.Run(ProcessLoopAsync);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref isDisposed, 1) != 0)
            return;

        messageChannel.Writer.TryComplete();
        disposalTokenSource.Cancel();

        try
        {
            processingTask.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            // ignored
        }

        trayIconThread.Dispose();
        disposalTokenSource.Dispose();
    }

    public void ShowBalloonTip(string title, string message, ToolTipIcon tooltipIcon = ToolTipIcon.Info)
    {
        if (IsDisposed)
            return;

        _ = messageChannel.Writer.TryWrite(new(title ?? string.Empty, message ?? string.Empty, tooltipIcon));
    }

    private async Task ProcessLoopAsync()
    {
        var reader = messageChannel.Reader;
        var token  = disposalTokenSource.Token;
        var buffer = new List<BalloonTipMessage>(8);

        try
        {
            while (await reader.WaitToReadAsync(token).ConfigureAwait(false))
            {
                buffer.Clear();

                while (reader.TryRead(out var message))
                    buffer.Add(message);

                if (buffer.Count == 0)
                    continue;

                await Task.Delay(MergeWindow, token).ConfigureAwait(false);

                while (reader.TryRead(out var message))
                    buffer.Add(message);

                if (ShouldSkipCurrentBatch())
                {
                    trayIconThread.Hide();
                    continue;
                }

                ShowBatch(buffer);
                await Task.Delay(DisplayWindow, token).ConfigureAwait(false);
                trayIconThread.Hide();
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // ignored
        }
        finally
        {
            trayIconThread.Hide();
        }
    }

    private void ShowBatch(List<BalloonTipMessage> buffer)
    {
        if (buffer.Count > 1)
        {
            var summary = string.Format(CultureInfo.CurrentCulture, MultiMessageTemplate, buffer.Count);
            trayIconThread.ShowBalloonTip(5000, summary, summary, ToolTipIcon.Info);
            return;
        }

        var message = buffer[0];
        trayIconThread.ShowBalloonTip(5000, message.Title, message.Message, message.Icon);
    }

    private bool ShouldSkipCurrentBatch() =>
        OnlyBackground && GameState.IsForeground;

    private sealed class TrayIconThread : IDisposable
    {
        private readonly ManualResetEventSlim initialized = new();
        private readonly Thread               thread;

        private WindowsFormsSynchronizationContext? context;
        private NotifyIcon?                         trayIcon;
        private Exception?                          initializationException;
        private int                                 isDisposed;

        public TrayIconThread()
        {
            thread = new(Run)
            {
                IsBackground = true,
                Name         = nameof(TrayNotifier)
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            initialized.Wait();

            if (initializationException is not null)
                throw new InvalidOperationException("无法初始化托盘通知线程。", initializationException);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref isDisposed, 1) != 0)
                return;

            if (Thread.CurrentThread == thread)
            {
                Application.ExitThread();
                return;
            }

            Volatile.Read(ref context)?.Post
            (
                static state =>
                {
                    var owner = (TrayIconThread)state!;

                    if (owner.trayIcon is { } icon)
                        icon.Visible = false;

                    Application.ExitThread();
                },
                this
            );

            thread.Join();
            initialized.Dispose();
        }

        public void SetIcon(Icon icon) =>
            Post
            (() =>
                {
                    if (trayIcon is not null)
                        trayIcon.Icon = icon;
                }
            );

        public void ShowBalloonTip(int timeout, string title, string message, ToolTipIcon tooltipIcon)
        {
            var resolvedMessage = string.IsNullOrWhiteSpace(message) ? title : message;
            if (string.IsNullOrWhiteSpace(resolvedMessage))
                return;

            Post
            (() =>
                {
                    if (trayIcon is not { Icon: not null } icon)
                        return;

                    icon.Visible = true;
                    icon.ShowBalloonTip(timeout, title, resolvedMessage, tooltipIcon);
                }
            );
        }

        public void Hide() =>
            Post
            (() =>
                {
                    if (trayIcon is not null)
                        trayIcon.Visible = false;
                }
            );

        private void Run()
        {
            try
            {
                using var synchronizationContext = new WindowsFormsSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);

                context  = synchronizationContext;
                trayIcon = new() { Visible = false };

                initialized.Set();
                Application.Run();
            }
            catch (Exception ex)
            {
                initializationException = ex;
                initialized.Set();
            }
            finally
            {
                trayIcon?.Dispose();
                trayIcon = null;

                SynchronizationContext.SetSynchronizationContext(null);
            }
        }

        private void Post(Action action)
        {
            if (Volatile.Read(ref isDisposed) != 0)
                return;

            Volatile.Read(ref context)?.Post
            (
                static state =>
                {
                    var (owner, callback) = ((TrayIconThread Owner, Action Callback))state!;

                    if (Volatile.Read(ref owner.isDisposed) != 0)
                        return;

                    callback();
                },
                (this, action)
            );
        }
    }

    private readonly record struct BalloonTipMessage
    (
        string      Title,
        string      Message,
        ToolTipIcon Icon
    );
}
