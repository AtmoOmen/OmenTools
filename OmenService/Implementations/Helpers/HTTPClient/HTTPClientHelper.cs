using System.Collections.Concurrent;
using System.Net;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public class HTTPClientHelper : OmenServiceBase<HTTPClientHelper>
{
    private const int LIFECYCLE_ACTIVE         = 0;
    private const int LIFECYCLE_UNINITIALIZING = 1;

    private readonly ConcurrentDictionary<string, HttpClient>            clients                  = [];
    private readonly ConcurrentDictionary<long, CancellationTokenSource> sharedCancellationLeases = [];

    private CancellationTokenSource sharedCancellationSource = new();
    private long                    sharedCancellationLeaseIdentity;
    private int                     lifecycleState = LIFECYCLE_ACTIVE;
    private int                     pendingAcquireCount;

    protected override void Uninit()
    {
        if (Interlocked.Exchange(ref lifecycleState, LIFECYCLE_UNINITIALIZING) != LIFECYCLE_ACTIVE)
            return;

        var rootSource = Interlocked.Exchange(ref sharedCancellationSource, CreateCanceledSource());
        WaitPendingAcquireCompleted();

        CancellationTokenSource[] leaseSources = [.. sharedCancellationLeases.Values];
        sharedCancellationLeases.Clear();

        HttpClient[] existedClients = [.. clients.Values];
        clients.Clear();

        TryCancel(rootSource);

        foreach (var leaseSource in leaseSources)
        {
            TryCancel(leaseSource);
            TryDispose(leaseSource);
        }

        TryDispose(rootSource);

        foreach (var client in existedClients)
            client.Dispose();
    }

    /// <summary>
    ///     租借一个与 <see cref="HTTPClientHelper" /> 生命周期绑定的共享取消令牌。
    ///     在 <see cref="Uninit" /> 时会统一取消并回收所有已发出的令牌。
    /// </summary>
    /// <param name="cancellationToken">可选的外部取消令牌，会与共享令牌自动联动。</param>
    /// <returns>请使用 <c>using var</c> 保存返回值，并通过 <see cref="SharedCancellationLease.Token" /> 取出令牌。</returns>
    public SharedCancellationLease AcquireSharedCancellation(CancellationToken cancellationToken = default)
    {
        if (Volatile.Read(ref lifecycleState) != LIFECYCLE_ACTIVE)
            return SharedCancellationLease.Canceled;

        Interlocked.Increment(ref pendingAcquireCount);

        try
        {
            if (Volatile.Read(ref lifecycleState) != LIFECYCLE_ACTIVE)
                return SharedCancellationLease.Canceled;

            var rootSource = Volatile.Read(ref sharedCancellationSource);
            var leaseSource = cancellationToken.CanBeCanceled
                                  ? CancellationTokenSource.CreateLinkedTokenSource(rootSource.Token, cancellationToken)
                                  : CancellationTokenSource.CreateLinkedTokenSource(rootSource.Token);

            var leaseID = Interlocked.Increment(ref sharedCancellationLeaseIdentity);
            sharedCancellationLeases[leaseID] = leaseSource;

            if (Volatile.Read(ref lifecycleState) != LIFECYCLE_ACTIVE)
            {
                if (sharedCancellationLeases.TryRemove(leaseID, out var removedSource))
                {
                    TryCancel(removedSource);
                    TryDispose(removedSource);
                }

                return SharedCancellationLease.Canceled;
            }

            return new
            (
                leaseSource.Token,
                () =>
                {
                    sharedCancellationLeases.TryRemove(leaseID, out _);
                    TryDispose(leaseSource);
                }
            );
        }
        catch (ObjectDisposedException)
        {
            return SharedCancellationLease.Canceled;
        }
        finally
        {
            Interlocked.Decrement(ref pendingAcquireCount);
        }
    }

    /// <summary>
    ///     获取一个 <see cref="HttpClient" /> 实例；如无特殊需求，尽量使用默认实例。
    /// </summary>
    /// <param name="name">建议带上前缀与用途，例如 <c>AutoAntiCensorShip.Default</c>。</param>
    public HttpClient Get(string name = "default") =>
        clients.GetOrAdd(name, static _ => CreateClient());

    /// <summary>
    ///     获取一个 <see cref="HttpClient" /> 实例；如无特殊需求，尽量使用默认实例。
    /// </summary>
    /// <param name="handler">自定义的 <see cref="HttpClientHandler" />。</param>
    /// <param name="name">建议带上前缀与用途，建议使用短横线分隔，例如 <c>AutoAntiCensorShip-Default</c>。</param>
    public HttpClient Get(HttpClientHandler handler, string name = "default") =>
        clients.GetOrAdd(name, static (_, state) => CreateClient(state), handler);

    /// <summary>
    ///     获取一个 <see cref="HttpClient" /> 实例，并在首次创建时执行额外配置。
    /// </summary>
    /// <param name="name">实例名称。</param>
    /// <param name="configure">首次创建实例时的配置逻辑。</param>
    public HttpClient Get(string name, Action<HttpClient> configure) =>
        clients.GetOrAdd(name, static (_, state) => CreateClient(configure: state), configure);

    /// <summary>
    ///     获取一个 <see cref="HttpClient" /> 实例，并在首次创建时执行额外配置。
    /// </summary>
    /// <param name="handler">自定义的 <see cref="HttpClientHandler" />。</param>
    /// <param name="name">实例名称。</param>
    /// <param name="configure">首次创建实例时的配置逻辑。</param>
    public HttpClient Get(HttpClientHandler handler, string name, Action<HttpClient> configure) =>
        clients.GetOrAdd(name, static (_, state) => CreateClient(state.Handler, state.Configure), (Handler: handler, Configure: configure));

    #region 辅助方法

    private static HttpClient CreateClient(HttpMessageHandler? handler = null, Action<HttpClient>? configure = null)
    {
        var client = handler is null ? new HttpClient() : new(handler);
        client.DefaultRequestVersion = HttpVersion.Version30;
        client.DefaultVersionPolicy  = HttpVersionPolicy.RequestVersionOrLower;
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/118.0");
        configure?.Invoke(client);
        return client;
    }

    private static CancellationTokenSource CreateCanceledSource()
    {
        var source = new CancellationTokenSource();
        source.Cancel();
        return source;
    }

    private void WaitPendingAcquireCompleted()
    {
        var spinner = new SpinWait();

        while (Volatile.Read(ref pendingAcquireCount) > 0)
            spinner.SpinOnce();
    }

    private static void TryCancel(CancellationTokenSource source)
    {
        try
        {
            source.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private static void TryDispose(CancellationTokenSource source)
    {
        try
        {
            source.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // ignored
        }
    }

    #endregion
}
