using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Timer = System.Timers.Timer;

namespace OmenTools.Helpers;

public sealed class TrayNotification : IDisposable
{
    /// <summary>
    ///     当短时间内收到多条信息时显示的聚合消息模板
    ///     <example>收到了 {0} 条新消息</example>
    /// </summary>
    public string MultiMessagesReceivedFormat { get; set; } = string.Empty;

    /// <summary>
    ///     是否仅当窗口在后台时才显示消息
    /// </summary>
    public bool OnlyBackground { get; set; }

    /// <summary>
    ///     显示通知期间临时在托盘栏显示的托盘图标
    /// </summary>
    public Icon Icon { get; set; }

    private NotifyIcon? Tray { get; set; }

    private readonly Queue<BalloonTipMessage> messageQueue = [];

    private readonly Lock queueLock = new();

    // Windows 10+ 中 BallonTooltip 显示多久全看用户电脑设置, 传入时长没用
    // 但是一般没人会改也不知道在哪改, 所以手动计时
    private Timer? displayTimer;
    private Timer? delayTimer;
    private bool   isDisposed;

    public TrayNotification
    (
        Icon   icon,
        string multiMessagesReceivedFormat = "收到了 {0} 条新消息",
        bool   onlyBackground              = false
    )
    {
        ArgumentNullException.ThrowIfNull(icon);

        Icon                        = icon;
        MultiMessagesReceivedFormat = multiMessagesReceivedFormat;
        OnlyBackground              = onlyBackground;
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        messageQueue.Clear();

        CleanupTray();

        Icon = null;
    }

    public void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        if (isDisposed) return;

        lock (queueLock)
        {
            messageQueue.Enqueue(new(title, message, icon));

            // 首次启动延迟
            if (delayTimer == null)
            {
                delayTimer         =  new Timer(500) { AutoReset = false };
                delayTimer.Elapsed += (_, _) => StartProcessing();
            }

            // 重置
            delayTimer.Stop();
            delayTimer.Start();
        }
    }

    private void StartProcessing()
    {
        lock (queueLock)
        {
            if (Tray == null)
            {
                Tray = new NotifyIcon { Icon = Icon };

                displayTimer         =  new Timer(7000) { AutoReset = false };
                displayTimer.Elapsed += (_, _) => ProcessNextMessage();
            }

            Tray.Visible = true;
            ProcessNextMessage();
        }
    }

    private void ProcessNextMessage()
    {
        lock (queueLock)
        {
            if (isDisposed || messageQueue.Count == 0)
            {
                CleanupTray();
                return;
            }

            // 检查是否只在后台显示通知且当前窗口不在后台
            if (OnlyBackground)
            {
                unsafe
                {
                    if (!Framework.Instance()->WindowInactive)
                    {
                        messageQueue.Clear();
                        CleanupTray();
                        return;
                    }
                }
            }

            // 剩余消息 >= 2 时显示汇总
            if (messageQueue.Count >= 2)
            {
                var count = messageQueue.Count;
                messageQueue.Clear();

                Tray.ShowBalloonTip
                (
                    5000,
                    string.Format(MultiMessagesReceivedFormat, count),
                    string.Format(MultiMessagesReceivedFormat, count),
                    ToolTipIcon.Info
                );
            }
            else
            {
                var msg = messageQueue.Dequeue();
                Tray.ShowBalloonTip(5000, msg.Title, msg.Message, msg.Icon);
            }

            // 下条消息计时
            displayTimer.Start();
        }
    }

    private void CleanupTray()
    {
        if (Tray == null) return;

        Tray.Visible = false;
        Tray.Dispose();
        Tray = null;

        displayTimer?.Dispose();
        displayTimer = null;

        delayTimer?.Dispose();
        delayTimer = null;
    }

    private record struct BalloonTipMessage(string Title, string Message, ToolTipIcon Icon);
}
