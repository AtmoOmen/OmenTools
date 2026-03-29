using System.Timers;
using Timer = System.Timers.Timer;

namespace OmenTools.Threading;

public class CountdownTimer : IDisposable
{
    private readonly Timer timer;
    private volatile int   remainingSeconds;

    public event EventHandler<int>? TimeChanged;
    public event EventHandler?      Completed;

    public int RemainingSeconds
    {
        get => remainingSeconds;
        private set => Interlocked.Exchange(ref remainingSeconds, value);
    }

    public bool IsRunning { get; private set; }

    public CountdownTimer(int totalSeconds)
    {
        timer            =  new Timer(1000) { AutoReset = true };
        timer.Elapsed    += OnTimerElapsed;
        RemainingSeconds =  totalSeconds;
    }

    public void Start()
    {
        if (IsRunning) return;
        IsRunning = true;
        timer.Start();
    }

    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        timer.Stop();
    }

    public void Reset(int newTotalSeconds)
    {
        Stop();
        RemainingSeconds = newTotalSeconds;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        int current;
        int newValue;

        do
        {
            current = RemainingSeconds;

            if (current <= 0)
            {
                Stop();
                Completed?.Invoke(this, EventArgs.Empty);
                return;
            }

            newValue = current - 1;
        }
        while (Interlocked.CompareExchange(ref remainingSeconds, newValue, current) != current);

        TimeChanged?.Invoke(this, newValue);

        if (newValue == 0)
        {
            Stop();
            Completed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Dispose() => timer.Dispose();
}
