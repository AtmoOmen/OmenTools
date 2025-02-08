using Timer = System.Timers.Timer;

namespace OmenTools.Helpers;

public static class TrayNotify
{
    private static NotifyIcon? Tray { get; set; }
    
    private static readonly Queue<BalloonTipMessage> MessageQueue = [];
    private static readonly object                   QueueLock    = new();
    private static          Icon?                    savedIcon;
    private static          Timer?                   displayTimer;
    private static          bool                     isDisposed;

    internal static void Init(Icon icon)
    {
        lock (QueueLock)
        {
            savedIcon?.Dispose();
            savedIcon = (Icon)icon.Clone();
        }
    }

    public static void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        if (isDisposed || savedIcon == null) return;

        lock (QueueLock)
        {
            MessageQueue.Enqueue(new BalloonTipMessage(title, message, icon));
            
            if (Tray == null)
            {
                Tray = new NotifyIcon
                {
                    Icon = savedIcon,
                    Visible = true
                };
                
                displayTimer = new Timer(3000) { AutoReset = false };
                displayTimer.Elapsed += (_, _) => ProcessNextMessage();
            }

            // 立即处理首个消息
            if (!displayTimer!.Enabled) 
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

            // 确保图标可见
            Tray ??= new NotifyIcon { Icon = savedIcon };
            Tray.Visible = true;

            var msg = MessageQueue.Dequeue();
            Tray.ShowBalloonTip(3000, msg.Title, msg.Message, msg.Icon);

            // 下条消息
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
