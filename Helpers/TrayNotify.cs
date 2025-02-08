using Timer = System.Timers.Timer;

namespace OmenTools.Helpers;

public static class TrayNotify
{
    private static NotifyIcon? Tray { get; set; }

    private static string MultiMessagesReceived { get; set; } = string.Empty;
    
    private static readonly Queue<BalloonTipMessage> MessageQueue = [];
    private static readonly object                   QueueLock    = new();
    private static          Icon?                    savedIcon;
    private static          Timer?                   displayTimer;
    private static          Timer?                   delayTimer;
    private static          bool                     isDisposed;

    internal static void Init(Icon icon, string multiMessagesReceived)
    {
        lock (QueueLock)
        {
            MultiMessagesReceived = multiMessagesReceived;
            
            savedIcon?.Dispose();
            savedIcon = icon;
        }
    }

    public static void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        if (isDisposed || savedIcon == null) return;

        lock (QueueLock)
        {
            MessageQueue.Enqueue(new BalloonTipMessage(title, message, icon));

            // 首次启动延迟
            if (delayTimer == null)
            {
                delayTimer = new Timer(500) { AutoReset = false };
                delayTimer.Elapsed += (_, _) => StartProcessing();
            }

            // 重置
            delayTimer.Stop();
            delayTimer.Start();
        }
    }

    private static void StartProcessing()
    {
        lock (QueueLock)
        {
            if (Tray == null)
            {
                Tray = new NotifyIcon { Icon = savedIcon };
                
                displayTimer = new Timer(5000) { AutoReset = false };
                displayTimer.Elapsed += (_, _) => ProcessNextMessage();
            }

            Tray.Visible = true;
            ProcessNextMessage();
        }
    }

    private static void ProcessNextMessage()
    {
        lock (QueueLock)
        {
            if (isDisposed || MessageQueue.Count == 0)
            {
                CleanupTray();
                return;
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
            displayTimer!.Start();
        }
    }

    private static void CleanupTray()
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

    internal static void Uninit()
    {
        lock (QueueLock)
        {
            isDisposed = true;
            MessageQueue.Clear();
            
            savedIcon?.Dispose();
            savedIcon = null;
            
            CleanupTray();
        }
    }

    private readonly struct BalloonTipMessage(string title, string message, ToolTipIcon icon)
    {
        public string Title { get; } = title;
        public string Message { get; } = message;
        public ToolTipIcon Icon { get; } = icon;
    }
}
