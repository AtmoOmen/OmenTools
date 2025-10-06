using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Timer = System.Timers.Timer;

namespace OmenTools.Helpers;

public static class TrayNotify
{
    private static NotifyIcon? Tray                  { get; set; }
    private static Icon?       Icon                  { get; set; }
    private static string      MultiMessagesReceived { get; set; } = string.Empty;
    private static bool        OnlyBackground        { get; set; }
    
    private static readonly Queue<BalloonTipMessage> MessageQueue = [];

    private static readonly Lock QueueLock = new();
    
    private static Timer? DisplayTimer;
    private static Timer? DelayTimer;
    private static bool   IsDisposed;

    internal static void Init(Icon icon, string multiMessagesReceived, bool onlyBackground = false)
    {
        MultiMessagesReceived = multiMessagesReceived;
        OnlyBackground        = onlyBackground;

        Icon ??= icon;
    }

    public static void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        if (IsDisposed || Icon == null) return;

        lock (QueueLock)
        {
            MessageQueue.Enqueue(new BalloonTipMessage(title, message, icon));

            // 首次启动延迟
            if (DelayTimer == null)
            {
                DelayTimer = new Timer(500) { AutoReset = false };
                DelayTimer.Elapsed += (_, _) => StartProcessing();
            }

            // 重置
            DelayTimer.Stop();
            DelayTimer.Start();
        }
    }

    private static void StartProcessing()
    {
        lock (QueueLock)
        {
            if (Tray == null)
            {
                Tray = new NotifyIcon { Icon = Icon };
                
                DisplayTimer = new Timer(7000) { AutoReset = false };
                DisplayTimer.Elapsed += (_, _) => ProcessNextMessage();
            }

            Tray.Visible = true;
            ProcessNextMessage();
        }
    }

    private static void ProcessNextMessage()
    {
        lock (QueueLock)
        {
            if (IsDisposed || MessageQueue.Count == 0)
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
                        MessageQueue.Clear();
                        CleanupTray();
                        return;
                    }
                }
            }

            // 剩余消息 >= 2 时显示汇总
            if (MessageQueue.Count >= 2)
            {
                var count = MessageQueue.Count;
                MessageQueue.Clear();

                Tray!.ShowBalloonTip(5000,
                                     string.Format(MultiMessagesReceived, count),
                                     string.Format(MultiMessagesReceived, count),
                                     ToolTipIcon.Info);
            }
            else
            {
                var msg = MessageQueue.Dequeue();
                Tray!.ShowBalloonTip(5000, msg.Title, msg.Message, msg.Icon);
            }

            // 下条消息计时
            DisplayTimer!.Start();
        }
    }

    private static void CleanupTray()
    {
        if (Tray == null) return;
        
        Tray.Visible = false;
        Tray.Dispose();
        Tray = null;
        
        DisplayTimer?.Dispose();
        DisplayTimer = null;
        
        DelayTimer?.Dispose();
        DelayTimer = null;
    }

    internal static void Uninit()
    {
        IsDisposed = true;
        MessageQueue.Clear();
        
        Icon = null;
        
        CleanupTray();
    }

    private readonly struct BalloonTipMessage(string title, string message, ToolTipIcon icon)
    {
        public string Title { get; } = title;
        public string Message { get; } = message;
        public ToolTipIcon Icon { get; } = icon;
    }
}
