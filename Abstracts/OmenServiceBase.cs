using Newtonsoft.Json;

namespace OmenTools.Abstracts;

public abstract class OmenServiceBase
{
    internal bool IsDisposed { get; private set; }
    
    internal string ConfigFilePath
    {
        get
        {
            var directory0 = Path.Join(DService.PI.GetPluginConfigDirectory(), "OmenTools");
            Directory.CreateDirectory(directory0);

            var directory1 = Path.Join(directory0, "Service");
            Directory.CreateDirectory(directory1);
            
            return Path.Join(directory1, $"{GetType().Name}.json");
        }
    }

    internal virtual void Init() { }
    
    internal virtual void Uninit() { }
    
    protected T LoadConfig<T>() where T : OmenServiceConfiguration => 
        LoadConfig<T>(GetType().Name);

    protected T LoadConfig<T>(string key) where T : OmenServiceConfiguration
    {
        try
        {
            if (!File.Exists(ConfigFilePath)) return null;
            var jsonString = File.ReadAllText(ConfigFilePath);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
        catch (Exception ex)
        {
            Error($"为 OmenService 加载配置失败: {key}", ex);
            return null;
        }
    }

    private object LoadConfig(Type T, string key)
    {
        if (!T.IsSubclassOf(typeof(OmenServiceConfiguration)))
            throw new Exception($"{T} 不继承 OmenServiceConfiguration 类");

        try
        {
            if (!File.Exists(ConfigFilePath)) return null;
            var jsonString = File.ReadAllText(ConfigFilePath);
            return JsonConvert.DeserializeObject(jsonString, T);
        }
        catch (Exception ex)
        {
            Error($"为 OmenService 加载配置失败: {key}", ex);
            return null;
        }
    }

    protected void SaveConfig<T>(T config) where T : OmenServiceConfiguration
    {
        try
        {
            var jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(ConfigFilePath, jsonString);
        }
        catch (Exception ex)
        {
            Error($"为 OmenService 加载配置失败: {GetType().Name}", ex);
        }
    }

    private void SaveConfig(object config)
    {
        try
        {
            if (!config.GetType().IsSubclassOf(typeof(OmenServiceConfiguration)))
            {
                Error($"保存配置失败: {config.GetType().Name} 不继承 OmenServiceConfiguration 类");
                return;
            }

            var jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, jsonString);
        }
        catch (Exception ex)
        {
            Error($"为 OmenService 加载配置失败: {GetType().Name}", ex);
        }
    }
    
    internal void SetDisposed() => IsDisposed = true;
}
