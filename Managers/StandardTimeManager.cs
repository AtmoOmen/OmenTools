using System.Net.Http;
using System.Text.RegularExpressions;
using GuerrillaNtp;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public partial class StandardTimeManager : OmenServiceBase<StandardTimeManager>
{
    public DateTime Today =>
        Now.Date;

    public DateTime Now =>
        UTCNow.ToLocalTime();

    public DateTime UTCNow
    {
        get
        {
            if (Clock != null)
                return Clock.UtcNow.UtcDateTime;

            if (WebAPIOffset.HasValue)
                return DateTime.UtcNow + WebAPIOffset.Value;

            return DateTime.UtcNow;
        }
    }

    public StandardTimeSource Source
    {
        get
        {
            if (Clock != null)
                return StandardTimeSource.NTP;

            if (WebAPIOffset.HasValue)
                return StandardTimeSource.Web;

            return StandardTimeSource.Local;
        }
    }


    private const string TIME_API_URL = "http://vv.video.qq.com/checktime?otype=json";

    private NtpClient  NTPClient  { get; } = new("ntp.ntsc.ac.cn");
    private HttpClient HTTPClient { get; } = new() { Timeout = TimeSpan.FromSeconds(5) };

    private NtpClock? Clock        { get; set; }
    private TimeSpan? WebAPIOffset { get; set; }

    private readonly CancellationTokenSource cancelSource = new();

    internal override void Init()
    {
        var token = cancelSource.Token;
        Task.Run
        (
            async () =>
            {
                try
                {
                    try
                    {
                        Clock = await NTPClient.QueryAsync(token).ConfigureAwait(false);
                    }
                    catch
                    {
                        await QueryWebTimeAsync(token).ConfigureAwait(false);
                    }
                }
                catch
                {
                    // ignored
                }
            },
            token
        );
    }

    private async Task QueryWebTimeAsync(CancellationToken token)
    {
        try
        {
            var response = await HTTPClient.GetStringAsync(TIME_API_URL, token).ConfigureAwait(false);

            var match = QQVideoResponseRegex().Match(response);

            if (match.Success && long.TryParse(match.Groups[1].Value, out var timestamp))
            {
                var serverTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
                WebAPIOffset = serverTime - DateTime.UtcNow;
            }
        }
        catch
        {
            // ignored
        }
    }

    internal override void Uninit()
    {
        if (!cancelSource.IsCancellationRequested)
            cancelSource.Cancel();
        cancelSource.Dispose();

        HTTPClient.Dispose();
    }

    [GeneratedRegex("\"t\":(\\d+)")]
    private static partial Regex QQVideoResponseRegex();
}

public enum StandardTimeSource
{
    NTP,
    Web,
    Local
}
