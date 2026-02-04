using System.Net.Http;
using System.Text.RegularExpressions;
using GuerrillaNtp;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public partial class StandardTimeManager : OmenServiceBase<StandardTimeManager>
{
    public DateTime Today =>
        TodayOffset.DateTime;

    public DateTimeOffset TodayOffset =>
        new(NowOffset.Date, NowOffset.Offset);

    public DateTime Now =>
        NowOffset.DateTime;

    public DateTimeOffset NowOffset =>
        UTCNowOffset.ToLocalTime();

    public DateTime UTCNow =>
        UTCNowOffset.UtcDateTime;

    public DateTimeOffset UTCNowOffset =>
        Clock?.UtcNow ??
        (WebAPIOffset.HasValue ? DateTimeOffset.UtcNow + WebAPIOffset.Value : DateTimeOffset.UtcNow);

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

    private NtpClient  NTPClient  { get; } = new("ntp.ntsc.ac.cn", TimeSpan.FromSeconds(5));
    private HttpClient HTTPClient { get; } = new() { Timeout = TimeSpan.FromSeconds(5) };

    private NtpClock? Clock        { get; set; }
    private TimeSpan? WebAPIOffset { get; set; }

    private readonly CancellationTokenSource cancelSource = new();

    internal override void Init()
    {
        var token = cancelSource.Token;
        _ = QueryAll(token);
    }

    private async Task QueryAll(CancellationToken token)
    {
        if (WebAPIOffset == null)
        {
            try
            {
                await QueryWebTimeAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            catch (Exception ex)
            {
                Error("尝试从 WebAPI 获取标准时间时发生错误", ex);
            }
        }

        if (Clock == null)
        {
            try
            {
                Clock = await NTPClient.QueryAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            catch (Exception ex)
            {
                Error("尝试从 ntp.ntsc.ac.cn 获取标准时间时发生错误", ex);

                try
                {
                    Clock = await NtpClient.Default.QueryAsync(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
                catch
                {
                    Error("尝试从 pool.ntp.org 获取标准时间时发生错误", ex);
                }
            }
        }

        Debug($"[StandardTimeManager] 请求标准时间完成, 当前 UTC 时间: {UTCNow}, 来源类型: {Source}");
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
