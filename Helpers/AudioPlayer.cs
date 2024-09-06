using WMPLib;

namespace OmenTools.Helpers;

public class AudioPlayer : IAsyncDisposable
{
    private readonly WindowsMediaPlayer player;
    private readonly TaskCompletionSource<bool> playbackStarted;
    private bool isDisposed;

    public event EventHandler<PlayStateChangedEventArgs>? PlayStateChanged;

    private AudioPlayer()
    {
        player = new WindowsMediaPlayer();
        player.PlayStateChange += Player_PlayStateChange;
        playbackStarted = new TaskCompletionSource<bool>();
    }

    public bool IsPlaying => player.playState == WMPPlayState.wmppsPlaying;

    public TimeSpan CurrentPosition => TimeSpan.FromSeconds(player.controls.currentPosition);

    public TimeSpan Duration => TimeSpan.FromSeconds(player.currentMedia?.duration ?? 0);

    private void Player_PlayStateChange(int newState)
    {
        var state = (WMPPlayState)newState;
        PlayStateChanged?.Invoke(this, new PlayStateChangedEventArgs(state));

        switch (state)
        {
            case WMPPlayState.wmppsPlaying:
                playbackStarted.TrySetResult(true);
                break;
            case WMPPlayState.wmppsStopped or WMPPlayState.wmppsMediaEnded:
                playbackStarted.TrySetResult(false);
                break;
        }
    }

    public static async Task PlayAudioAsync(string filePath, int timeoutSeconds = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(@"文件路径为 Null 或空", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("未找到音频文件", filePath);

        await using var player = new AudioPlayer();
        await player.PlayInternalAsync(filePath, timeoutSeconds, cancellationToken);
    }

    private async Task PlayInternalAsync(string filePath, int timeoutSeconds, CancellationToken cancellationToken)
    {
        try
        {
            player.URL = filePath;
            player.controls.play();

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            await playbackStarted.Task.WaitAsync(timeoutCts.Token);

            if (!playbackStarted.Task.Result) return;
            
            while (IsPlaying && !cancellationToken.IsCancellationRequested)
                await Task.Delay(100, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            DService.Log.Error("音频播放操作被取消或超时");
        }
        catch (Exception ex)
        {
            DService.Log.Error("音频播放过程中发生错误:", ex);
        }
    }

    private void SetVolume(int volume)
    {
        if (volume is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(volume), "音量必须为 0 到 100 之间");

        player.settings.volume = volume;
    }

    public async ValueTask DisposeAsync()
    {
        if (!isDisposed)
        {
            await Task.Run(() =>
            {
                player.PlayStateChange -= Player_PlayStateChange;
                player.controls.stop();
                player.close();
            });
            isDisposed = true;

        }
    }
    
    public class PlayStateChangedEventArgs(WMPPlayState newState) : EventArgs
    {
        public WMPPlayState NewState { get; } = newState;
    }
}
