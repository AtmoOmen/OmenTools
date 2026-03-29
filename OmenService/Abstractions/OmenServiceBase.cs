using Newtonsoft.Json;
using OmenTools.Dalamud;

namespace OmenTools.OmenService.Abstractions;

public abstract class OmenServiceBase<T> : OmenServiceBase where T : OmenServiceBase<T>
{
    public static T Instance() =>
        DService.Instance().GetOmenService<T>() ??
        throw new InvalidOperationException($"服务 {typeof(T).Name} 尚未注册或初始化");
}

public abstract class OmenServiceBase
{
    internal string ConfigFilePath
    {
        get
        {
            var directory0 = Path.Join(DService.Instance().PI.GetPluginConfigDirectory(), "OmenTools");
            Directory.CreateDirectory(directory0);

            var directory1 = Path.Join(directory0, "Service");
            Directory.CreateDirectory(directory1);

            return Path.Join(directory1, $"{GetType().Name}.json");
        }
    }

    internal void PublicInit()
    {
        if (IsDisposed || IsInitialized)
            return;

        try
        {
            Init();

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            DLog.Error($"初始化 OmenService 时发生错误: {GetType().Name}", ex);
            PublicUninit();
        }
    }

    internal void PublicUninit()
    {
        if (IsDisposed)
            return;

        try
        {
            Uninit();
        }
        catch (Exception ex)
        {
            DLog.Error($"卸载 OmenService 时发生错误: {GetType().Name}", ex);
        }
        finally
        {
            IsDisposed = true;
        }
    }

    #region 生命周期控制

    internal bool IsDisposed { get; private set; }

    internal bool IsInitialized { get; private set; }

    #endregion

    #region 继承

    protected virtual void Init() { }

    protected virtual void Uninit() { }

    #endregion

    #region 配置

    internal T? LoadConfig<T>() where T : OmenServiceConfig
    {
        try
        {
            if (!File.Exists(ConfigFilePath)) return null;
            var jsonString = File.ReadAllText(ConfigFilePath);
            return JsonConvert.DeserializeObject<T>(jsonString, JsonSerializerSettings.GetShared());
        }
        catch (Exception ex)
        {
            DLog.Error($"加载 OmenService 配置失败: {GetType().Name}", ex);
            return null;
        }
    }

    internal void SaveConfig<T>(T config) where T : OmenServiceConfig
    {
        try
        {
            ArgumentNullException.ThrowIfNull(config);

            var jsonString = JsonConvert.SerializeObject(config, Formatting.Indented, JsonSerializerSettings.GetShared());
            SecureSaveHelper.Instance().WriteAllText(ConfigFilePath, jsonString);
        }
        catch (Exception ex)
        {
            DLog.Error($"保存 OmenService 配置失败: {GetType().Name}", ex);
        }
    }

    #endregion
}
