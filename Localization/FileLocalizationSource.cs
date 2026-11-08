using Lumina.Data;
using OmenTools.Localization.Abstractions;

namespace OmenTools.Localization;

/// <summary>
/// 从指定目录读取本地化资源，并监视资源文件的变化。
/// </summary>
/// <param name="directoryPath">本地化资源所在的目录。</param>
/// <param name="streamTransform">读取资源时应用的流转换；转换后的流负责释放原始文件流。</param>
public sealed class FileLocalizationSource
(
    string                directoryPath,
    Func<Stream, Stream>? streamTransform = null
) : ILocalizationSource
{
    private readonly string directoryPath = Path.GetFullPath(directoryPath);

    private FileSystemWatcher?                                watcher;
    private EventHandler<LocalizationSourceChangedEventArgs>? resourceChanged;

    /// <summary>
    /// 获取是否支持资源文件变化通知；始终返回 <see langword="true" />。
    /// </summary>
    public bool SupportsChangeNotifications => true;

    /// <summary>
    /// 在资源文件创建、修改、重命名或移除时触发；首次订阅时开始监视，取消全部订阅时停止监视。
    /// </summary>
    public event EventHandler<LocalizationSourceChangedEventArgs>? ResourceChanged
    {
        add
        {
            resourceChanged += value;
            EnsureWatcher();
        }
        remove
        {
            resourceChanged -= value;

            if (resourceChanged == null)
                DisposeWatcher();
        }
    }

    /// <summary>
    /// 检查指定名称的资源文件是否存在。
    /// </summary>
    /// <param name="language">资源所属的语言；实际文件位置由资源名称确定。</param>
    /// <param name="resourceName">相对于资源目录的文件名称。</param>
    /// <returns>资源文件存在时返回 <see langword="true" />，否则返回 <see langword="false" />。</returns>
    public bool Exists(Language language, string resourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        return File.Exists(GetFilePath(resourceName));
    }

    /// <summary>
    /// 打开资源文件，并应用构造时指定的流转换。
    /// </summary>
    /// <param name="language">资源所属的语言；实际文件位置由资源名称确定。</param>
    /// <param name="resourceName">相对于资源目录的文件名称。</param>
    /// <returns>由调用方负责释放的资源流；文件不存在时返回 <see langword="null" />。</returns>
    public Stream? OpenRead(Language language, string resourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);

        var filePath = GetFilePath(resourceName);
        if (!File.Exists(filePath))
            return null;

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

        try
        {
            return streamTransform == null ?
                       stream :
                       streamTransform(stream);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 停止文件变化监视并释放监视器。
    /// </summary>
    public void Dispose() =>
        DisposeWatcher();

    private string GetFilePath(string resourceName) =>
        Path.Join(directoryPath, resourceName);

    private void EnsureWatcher()
    {
        if (watcher != null)
            return;

        Directory.CreateDirectory(directoryPath);

        watcher = new(directoryPath)
        {
            Filter                = "*.*",
            IncludeSubdirectories = false,
            NotifyFilter          = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
            EnableRaisingEvents   = true
        };

        watcher.Changed += OnWatcherChanged;
        watcher.Created += OnWatcherChanged;
        watcher.Renamed += OnWatcherChanged;
        watcher.Deleted += OnWatcherChanged;
    }

    private void DisposeWatcher()
    {
        if (watcher == null)
            return;

        watcher.Changed -= OnWatcherChanged;
        watcher.Created -= OnWatcherChanged;
        watcher.Renamed -= OnWatcherChanged;
        watcher.Deleted -= OnWatcherChanged;
        watcher.Dispose();
        watcher = null;
    }

    private void OnWatcherChanged(object sender, FileSystemEventArgs e)
    {
        var handler = resourceChanged;
        handler?.Invoke(this, new(e.Name ?? string.Empty, e.ChangeType));
    }
}
