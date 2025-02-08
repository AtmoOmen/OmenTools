using Timer = System.Timers.Timer;

namespace OmenTools.Helpers;

public static class TrayNotify
{
    private static NotifyIcon? Tray { get; set; }
    
    private static readonly Queue<BalloonTipMessage> MessageQueue = [];
    private static readonly object QueueLock = new();
    private static Icon? _savedIcon;
    private static Timer? _displayTimer;
    private static bool _isDisposed;
    private static bool _isFirstMessage = true;

    internal static void Init(Icon icon)
    {
        lock (QueueLock)
        {
            _savedIcon?.Dispose();
            _savedIcon = (Icon)icon.Clone();
        }
    }

    public static void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        if (_isDisposed || _savedIcon == null) return;

        lock (QueueLock)
        {
            MessageQueue.Enqueue(new BalloonTipMessage(title, message, icon));
            
            if (Tray == null)
            {
                CreateTray();
                _displayTimer = new Timer { AutoReset = false };
                _displayTimer.Elapsed += (_, _) => ProcessNextMessage();
            }

            if (!_displayTimer!.Enabled)
            {
                // 首条消息延迟500ms
                _displayTimer.Interval = _isFirstMessage ? 500 : 3000;
                _displayTimer.Start();
            }
        }
    }

    private static void ProcessNextMessage()
    {
        lock (QueueLock)
        {
            if (_isDisposed || MessageQueue.Count == 0)
            {
                CleanupTray();
                return;
            }

            // 处理消息合并逻辑
            var remaining = MessageQueue.Count - 1;
            if (remaining >= 2)
            {
                var msg = MessageQueue.Dequeue();
                var mergedMsg = new BalloonTipMessage(msg.Title, $"收到了 {remaining + 1} 条新消息", msg.Icon);
                
                MessageQueue.Clear();
                MessageQueue.Enqueue(mergedMsg);
            }

            CreateTray();
            var currentMsg = MessageQueue.Dequeue();
            Tray!.ShowBalloonTip(3000, currentMsg.Title, currentMsg.Message, currentMsg.Icon);

            // 更新定时器间隔
            _isFirstMessage = false;
            _displayTimer!.Interval = 3000;
            
            if (MessageQueue.Count > 0)
            {
                _displayTimer.Start();
            }
            else
            {
                CleanupTray();
            }
        }
    }

    private static void CreateTray()
    {
        if (Tray == null)
        {
            Tray = new NotifyIcon { Icon = _savedIcon };
            Tray.Visible = true;
        }
    }

    private static void CleanupTray()
    {
        if (Tray == null) return;
        
        Tray.Visible = false;
        Tray.Dispose();
        Tray = null;
        
        _displayTimer?.Dispose();
        _displayTimer = null;
        _isFirstMessage = true; // 重置首条消息状态
    }

    internal static void Uninit()
    {
        lock (QueueLock)
        {
            _isDisposed = true;
            MessageQueue.Clear();
            _savedIcon?.Dispose();
            _savedIcon = null;
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
