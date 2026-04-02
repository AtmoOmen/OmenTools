using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Data;
using OmenTools.Dalamud;
using OmenTools.Localization;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public sealed class LocalizationManager : OmenServiceBase<LocalizationManager>
{
    private static readonly FrozenDictionary<string, string> EmptyResource =
        new Dictionary<string, string>(StringComparer.Ordinal).ToFrozenDictionary(StringComparer.Ordinal);

    private static readonly ConfiguredState EmptyState = new(null, null, false);

    private ConfiguredState          configuredState = EmptyState;
    private LanguageSnapshot         currentSnapshot = LanguageSnapshot.Empty;
    private CancellationTokenSource? reloadCancelSource;

    public bool IsConfigured =>
        Volatile.Read(ref configuredState).IsConfigured;

    public FrozenDictionary<Language, string> SupportedLanguages =>
        GetConfiguredState().Options!.SupportedLanguages;

    public FrozenDictionary<Language, string> AvailableLanguages =>
        GetSnapshot().AvailableLanguages;

    public Language CurrentLanguage =>
        GetSnapshot().Language;

    protected override void Uninit()
    {
        ResetConfiguredState();
        Interlocked.Exchange(ref currentSnapshot, LanguageSnapshot.Empty);
    }

    public void Configure(LocalizationOptions options, Language initialLanguage, Language preferredLanguage)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);

        var nextState     = new ConfiguredState(options, null, true);
        var previousState = Interlocked.Exchange(ref configuredState, nextState);

        try
        {
            CancelReload();
            DisposeConfiguredState(previousState);

            if (options.EnableHotReload && options.Source.SupportsChangeNotifications)
            {
                EventHandler<LocalizationSourceChangedEventArgs> handler = OnLocalizationSourceChanged;
                options.Source.ResourceChanged += handler;

                Volatile.Write(ref configuredState, nextState with { ChangeHandler = handler });
            }

            var normalizedLanguage = NormalizeLanguage(initialLanguage, preferredLanguage);
            LoadLanguage(normalizedLanguage);
        }
        catch
        {
            CancelReload();
            DisposeConfiguredState(Volatile.Read(ref configuredState));
            Volatile.Write(ref configuredState, EmptyState);
            Interlocked.Exchange(ref currentSnapshot, LanguageSnapshot.Empty);
            throw;
        }
    }

    public Language NormalizeLanguage(Language requestedLanguage, Language preferredLanguage)
    {
        var state              = GetConfiguredState();
        var options            = state.Options!;
        var availableLanguages = EnumerateAvailableLanguages(options);

        if (IsLanguageAvailable(options, availableLanguages, requestedLanguage))
            return requestedLanguage;

        if (IsLanguageAvailable(options, availableLanguages, preferredLanguage))
            return preferredLanguage;

        foreach (var fallbackLanguage in EnumerateFallbackLanguages(options, requestedLanguage))
        {
            if (IsLanguageAvailable(options, availableLanguages, fallbackLanguage))
                return fallbackLanguage;
        }

        foreach (var fallbackLanguage in EnumerateFallbackLanguages(options, preferredLanguage))
        {
            if (IsLanguageAvailable(options, availableLanguages, fallbackLanguage))
                return fallbackLanguage;
        }

        return options.DefaultLanguage;
    }

    public void LoadLanguage(Language language)
    {
        var state   = GetConfiguredState();
        var options = state.Options!;

        if (!options.SupportedLanguages.ContainsKey(language))
            throw new ArgumentOutOfRangeException(nameof(language), $"未在已配置语言列表中找到语言 {language}");

        var snapshot = BuildSnapshot(options, language);
        Interlocked.Exchange(ref currentSnapshot, snapshot);
    }

    public string Get(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var snapshot = GetSnapshot();
        return TryResolveFormat(snapshot, key, out var format) ? format : LogMissingKeyAndReturnKey(snapshot, key);
    }

    public string Get(string key, params object[] args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0)
            return Get(key);

        var snapshot = GetSnapshot();
        if (!TryResolveFormat(snapshot, key, out var format))
            return LogMissingKeyAndReturnKey(snapshot, key);

        return string.Format(CultureInfo.CurrentCulture, format, args);
    }

    public SeString GetSe(string key, params object[] args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(args);

        var snapshot = GetSnapshot();
        if (!TryResolveFormat(snapshot, key, out var format))
            return CreatePlainSeString(LogMissingKeyAndReturnKey(snapshot, key));

        var template = snapshot.TemplateCache.GetOrAdd(key, _ => SeTemplate.Compile(format));

        if (args.Length == 0 && template.IsLiteralOnly)
            return snapshot.PlainTextSeCache.GetOrAdd(key, _ => CreatePlainSeString(format));

        var builder = new SeStringBuilder();

        foreach (var segment in template.Segments)
        {
            if (!segment.IsArgument)
            {
                builder.AddText(segment.Text);
                continue;
            }

            if ((uint)segment.ArgumentIndex < (uint)args.Length)
            {
                AppendArgument(builder, args[segment.ArgumentIndex]);
                continue;
            }

            LogFormatError(snapshot, key, segment.Text);
            builder.AddText(segment.Text);
        }

        return builder.Build();
    }

    private ConfiguredState GetConfiguredState()
    {
        var state = Volatile.Read(ref configuredState);
        if (state.IsConfigured)
            return state;

        throw new InvalidOperationException("本地化服务尚未配置，请先调用 Configure(...)");
    }

    private LanguageSnapshot GetSnapshot()
    {
        _ = GetConfiguredState();
        return Volatile.Read(ref currentSnapshot);
    }

    private static void ValidateOptions(LocalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options.SupportedLanguages);
        ArgumentNullException.ThrowIfNull(options.FileNameResolver);
        ArgumentNullException.ThrowIfNull(options.Source);
        ArgumentNullException.ThrowIfNull(options.Parser);
        ArgumentNullException.ThrowIfNull(options.FallbackResolver);

        if (options.SupportedLanguages.Count == 0)
            throw new ArgumentException("必须至少配置一种支持语言", nameof(options));

        if (!options.SupportedLanguages.ContainsKey(options.DefaultLanguage))
            throw new ArgumentException($"默认语言 {options.DefaultLanguage} 不在支持语言列表中", nameof(options));

        if (options.ReloadDebounce < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "热重载防抖时间不能小于 0");

        ArgumentException.ThrowIfNullOrWhiteSpace(options.LoggerTag);
    }

    private void ResetConfiguredState()
    {
        CancelReload();

        var state = Interlocked.Exchange(ref configuredState, EmptyState);
        DisposeConfiguredState(state);
    }

    private void CancelReload()
    {
        var current = Interlocked.Exchange(ref reloadCancelSource, null);
        current?.Cancel();
        current?.Dispose();
    }

    private static void DisposeConfiguredState(ConfiguredState state)
    {
        if (!state.IsConfigured || state.Options == null)
            return;

        if (state.ChangeHandler != null)
            state.Options.Source.ResourceChanged -= state.ChangeHandler;

        state.Options.Source.Dispose();
    }

    private void OnLocalizationSourceChanged(object? sender, LocalizationSourceChangedEventArgs e)
    {
        var state = Volatile.Read(ref configuredState);
        if (!state.IsConfigured || state.Options == null)
            return;

        CancelReload();

        var cancelSource = new CancellationTokenSource();
        var previous     = Interlocked.Exchange(ref reloadCancelSource, cancelSource);
        previous?.Cancel();
        previous?.Dispose();

        DService.Instance().Framework.RunOnTick
        (
            () =>
            {
                if (cancelSource.IsCancellationRequested)
                    return;

                var currentState = Volatile.Read(ref configuredState);
                if (!currentState.IsConfigured || currentState.Options == null)
                    return;

                DLog.Debug($"[{currentState.Options.LoggerTag}] 本地化资源 {e.ResourceName} ({e.ChangeType}) 发生变动，开始重载当前语言");

                try
                {
                    LoadLanguage(CurrentLanguage);
                }
                catch (Exception ex)
                {
                    DLog.Error($"[{currentState.Options.LoggerTag}] 重载当前语言失败", ex);
                }
            },
            state.Options.ReloadDebounce,
            cancellationToken: cancelSource.Token
        );
    }

    private LanguageSnapshot BuildSnapshot(LocalizationOptions options, Language language)
    {
        var resourceLanguages = EnumerateResourceLanguages(options, language);
        var resources         = new List<FrozenDictionary<string, string>>(resourceLanguages.Count);

        foreach (var resourceLanguage in resourceLanguages)
        {
            var resource = LoadLanguageResource(options, resourceLanguage);
            if (resource.Count == 0)
                continue;

            resources.Add(resource);
        }

        return new(language, EnumerateAvailableLanguages(options), [.. resources], options.LoggerTag);
    }

    private static List<Language> EnumerateResourceLanguages(LocalizationOptions options, Language language)
    {
        HashSet<Language> deduped = [];
        List<Language>    ordered = [];

        AddLanguage(language);

        foreach (var fallbackLanguage in EnumerateFallbackLanguages(options, language))
            AddLanguage(fallbackLanguage);

        AddLanguage(options.DefaultLanguage);

        return ordered;

        void AddLanguage(Language value)
        {
            if (!options.SupportedLanguages.ContainsKey(value))
                return;

            if (!deduped.Add(value))
                return;

            ordered.Add(value);
        }
    }

    private static IEnumerable<Language> EnumerateFallbackLanguages(LocalizationOptions options, Language language)
    {
        if (!options.SupportedLanguages.ContainsKey(language))
            yield break;

        foreach (var fallbackLanguage in options.FallbackResolver(language))
            yield return fallbackLanguage;
    }

    private static bool IsLanguageAvailable(LocalizationOptions options, FrozenDictionary<Language, string> availableLanguages, Language language) =>
        options.SupportedLanguages.ContainsKey(language) && availableLanguages.ContainsKey(language);

    private static bool TryResolveFormat(LanguageSnapshot snapshot, string key, out string format)
    {
        foreach (var resource in snapshot.Resources)
        {
            if (resource.TryGetValue(key, out format))
                return true;
        }

        format = string.Empty;
        return false;
    }

    private FrozenDictionary<string, string> LoadLanguageResource(LocalizationOptions options, Language language)
    {
        try
        {
            var resourceName = options.FileNameResolver(language);
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new InvalidOperationException($"语言 {language} 的资源文件名不能为空");

            using var stream = options.Source.OpenRead(language, resourceName);
            if (stream == null)
                return EmptyResource;

            var resource = options.Parser.Parse(stream);
            return resource.Count == 0 ? EmptyResource : resource;
        }
        catch (Exception ex)
        {
            DLog.Error($"[{options.LoggerTag}] 读取 {language} 语言数据失败", ex);
            return EmptyResource;
        }
    }

    private FrozenDictionary<Language, string> EnumerateAvailableLanguages(LocalizationOptions options)
    {
        Dictionary<Language, string> availableLanguages = [];

        foreach (var language in options.SupportedLanguages)
        {
            var resourceName = options.FileNameResolver(language.Key);
            if (string.IsNullOrWhiteSpace(resourceName))
                continue;

            if (!options.Source.Exists(language.Key, resourceName))
                continue;

            availableLanguages[language.Key] = language.Value;
        }

        return availableLanguages.Count == 0
                   ? FrozenDictionary<Language, string>.Empty
                   : availableLanguages.ToFrozenDictionary();
    }

    private static void AppendArgument(SeStringBuilder builder, object? arg)
    {
        switch (arg)
        {
            case null:
                return;

            case SeString seString:
                builder.Append(seString);
                return;

            case SeStringBuilder seStringBuilder:
                builder.Append(seStringBuilder.Build());
                return;

            case Payload payload:
                builder.Add(payload);
                return;

            case BitmapFontIcon icon:
                builder.AddIcon(icon);
                return;

            case SeIconChar iconChar:
                builder.AddText(iconChar.ToIconString());
                return;

            case IFormattable formattable:
                builder.AddText(formattable.ToString(null, CultureInfo.CurrentCulture) ?? string.Empty);
                return;

            default:
                builder.AddText(arg.ToString() ?? string.Empty);
                return;
        }
    }

    private static SeString CreatePlainSeString(string text)
    {
        var builder = new SeStringBuilder();
        builder.AddText(text);
        return builder.Build();
    }

    private static string LogMissingKeyAndReturnKey(LanguageSnapshot snapshot, string key)
    {
        if (snapshot.MissingKeys.TryAdd(key, 0))
            DLog.Error($"[{snapshot.LoggerTag}] 未在当前语言链中找到本地化键 {key}");

        return key;
    }

    private static void LogFormatError(LanguageSnapshot snapshot, string key, string token)
    {
        var logKey = $"{key}|{token}";
        if (snapshot.FormatErrors.TryAdd(logKey, 0))
            DLog.Warning($"[{snapshot.LoggerTag}] 本地化键 {key} 缺少占位符参数 {token}，已保留原始文本");
    }

    private sealed record ConfiguredState
    (
        LocalizationOptions?                              Options,
        EventHandler<LocalizationSourceChangedEventArgs>? ChangeHandler,
        bool                                              IsConfigured
    );

    private sealed class LanguageSnapshot
    (
        Language                           language,
        FrozenDictionary<Language, string> availableLanguages,
        FrozenDictionary<string, string>[] resources,
        string                             loggerTag
    )
    {
        public static LanguageSnapshot Empty { get; } =
            new(Language.None, FrozenDictionary<Language, string>.Empty, [], nameof(LocalizationManager));

        public Language Language { get; } = language;

        public FrozenDictionary<Language, string> AvailableLanguages { get; } = availableLanguages;

        public FrozenDictionary<string, string>[] Resources { get; } = resources;

        public string LoggerTag { get; } = loggerTag;

        public ConcurrentDictionary<string, SeTemplate> TemplateCache { get; } = new(StringComparer.Ordinal);

        public ConcurrentDictionary<string, SeString> PlainTextSeCache { get; } = new(StringComparer.Ordinal);

        public ConcurrentDictionary<string, byte> MissingKeys { get; } = new(StringComparer.Ordinal);

        public ConcurrentDictionary<string, byte> FormatErrors { get; } = new(StringComparer.Ordinal);
    }

    private readonly record struct TemplateSegment
    (
        string Text,
        int    ArgumentIndex,
        bool   IsArgument
    );

    private sealed class SeTemplate
    (
        TemplateSegment[] segments,
        bool              isLiteralOnly
    )
    {
        public TemplateSegment[] Segments { get; } = segments;

        public bool IsLiteralOnly { get; } = isLiteralOnly;

        public static SeTemplate Compile(string format)
        {
            List<TemplateSegment> segments       = [];
            var                   literalBuilder = new StringBuilder();
            var                   isLiteralOnly  = true;

            for (var index = 0; index < format.Length;)
            {
                var current = format[index];

                if (current == '{')
                {
                    if (index + 1 < format.Length && format[index + 1] == '{')
                    {
                        literalBuilder.Append('{');
                        index += 2;
                        continue;
                    }

                    if (TryReadPlaceholder(format, index, out var nextIndex, out var argumentIndex))
                    {
                        FlushLiteral(segments, literalBuilder);
                        segments.Add(new(format[index..nextIndex], argumentIndex, true));
                        isLiteralOnly = false;
                        index         = nextIndex;
                        continue;
                    }
                }
                else if (current == '}' && index + 1 < format.Length && format[index + 1] == '}')
                {
                    literalBuilder.Append('}');
                    index += 2;
                    continue;
                }

                literalBuilder.Append(current);
                index++;
            }

            FlushLiteral(segments, literalBuilder);

            if (segments.Count == 0)
                segments.Add(new(string.Empty, -1, false));

            return new(segments.ToArray(), isLiteralOnly);
        }

        private static bool TryReadPlaceholder(string format, int startIndex, out int nextIndex, out int argumentIndex)
        {
            nextIndex     = startIndex;
            argumentIndex = -1;

            var index = startIndex + 1;
            if (index >= format.Length || !char.IsAsciiDigit(format[index]))
                return false;

            var value = 0;

            while (index < format.Length && char.IsAsciiDigit(format[index]))
            {
                value = value * 10 + format[index] - '0';
                index++;
            }

            if (index >= format.Length || format[index] != '}')
                return false;

            argumentIndex = value;
            nextIndex     = index + 1;
            return true;
        }

        private static void FlushLiteral(List<TemplateSegment> segments, StringBuilder literalBuilder)
        {
            if (literalBuilder.Length == 0)
                return;

            segments.Add(new(literalBuilder.ToString(), -1, false));
            literalBuilder.Clear();
        }
    }
}
