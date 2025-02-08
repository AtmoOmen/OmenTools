using Timer = System.Timers.Timer;

namespace OmenTools.Helpers;

public static class TrayNotify
{
    private static NotifyIcon? Tray { get; set; }

    private static readonly Queue<BalloonTipMessage> MessageQueue = [];
    private static readonly object                   QueueLock    = new();
    private static          Timer?                   displayTimer;
    private static          bool                     isDisposed;

    internal static void Init(Icon icon)
    {
        if (Tray != null) return;

        Tray = new NotifyIcon
        {
            Icon    = icon,
            Visible = false
        };

        // 初始化定时器（间隔保持3秒）
        displayTimer         =  new Timer(3000) { AutoReset = false };
        displayTimer.Elapsed += (_, _) => ProcessNextMessage();
    }

    public static void ShowBalloonTip(string title, string message, ToolTipIcon icon)
    {
        if (Tray == null || isDisposed) return;

        lock (QueueLock)
        {
            MessageQueue.Enqueue(new BalloonTipMessage(title, message, icon));

            // 如果当前没有正在显示的消息，立即处理
            if (!displayTimer!.Enabled) ProcessNextMessage();
        }
    }

    private static void ProcessNextMessage()
    {
        lock (QueueLock)
        {
            if (MessageQueue.Count == 0)
            {
                // 队列为空时隐藏图标
                Tray!.Visible = false;
                return;
            }

            var msg = MessageQueue.Dequeue();
            Tray!.Visible = true;
            Tray.ShowBalloonTip(3000, msg.Title, msg.Message, msg.Icon);

            // 启动定时器处理下一个消息
            displayTimer!.Start();
        }
    }

    internal static void Uninit()
    {
        isDisposed = true;

        lock (QueueLock)
        {
            displayTimer?.Stop();
            displayTimer?.Dispose();

            MessageQueue.Clear();

            if (Tray != null)
            {
                Tray.Visible = false;
                Tray.Dispose();
                Tray = null;
            }
        }
    }

    private readonly struct BalloonTipMessage(string title, string message, ToolTipIcon icon)
    {
        public string      Title   { get; } = title;
        public string      Message { get; } = message;
        public ToolTipIcon Icon    { get; } = icon;
    }
}
