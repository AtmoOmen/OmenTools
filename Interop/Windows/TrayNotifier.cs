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
        get => trayIcon.Icon ?? throw new InvalidOperationException("托盘图标尚未初始化。");
        set
        {
            if (IsDisposed)
                return;

            ArgumentNullException.ThrowIfNull(value);

            try
            {
                trayIcon.Icon = value;
            }
            catch (ObjectDisposedException)
            {
                // ignored
            }
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
    private readonly NotifyIcon              trayIcon;

    private int isDisposed;

    public TrayNotifier()
    {
        trayIcon = new() { Visible = false };

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

        trayIcon.Visible = false;
        trayIcon.Dispose();
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
                    trayIcon.Visible = false;
                    continue;
                }

                ShowBatch(buffer);
                await Task.Delay(DisplayWindow, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // ignored
        }
        finally
        {
            trayIcon.Visible = false;
        }
    }

    private void ShowBatch(List<BalloonTipMessage> buffer)
    {
        trayIcon.Visible = true;

        if (buffer.Count > 1)
        {
            var summary = string.Format(CultureInfo.CurrentCulture, MultiMessageTemplate, buffer.Count);
            trayIcon.ShowBalloonTip(5000, summary, summary, ToolTipIcon.Info);
            return;
        }

        var message = buffer[0];
        trayIcon.ShowBalloonTip(5000, message.Title, message.Message, message.Icon);
    }

    private bool ShouldSkipCurrentBatch() =>
        OnlyBackground && GameState.IsForeground;

    private readonly record struct BalloonTipMessage
    (
        string      Title,
        string      Message,
        ToolTipIcon Icon
    );
}
