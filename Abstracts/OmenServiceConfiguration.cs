using System.IO;
using Newtonsoft.Json;

namespace OmenTools.Abstracts;

public abstract class OmenServiceConfiguration;

public static class OmenServiceConfigurationExtensions
{
    public static T Load<T>(this T config, OmenServiceBase module) where T : OmenServiceConfiguration
    {
        try
        {
            var configFile = module.ConfigFilePath;
            if (!File.Exists(configFile)) return config;
            var jsonString = File.ReadAllText(configFile);
            return JsonConvert.DeserializeObject<T>(jsonString) ?? config;
        }
        catch (Exception ex)
        {
            Error($"加载 OmenService {typeof(T).Name} 配置失败", ex);
            return config;
        }
    }
    
    public static void Save(this OmenServiceConfiguration config, OmenServiceBase manager)
    {
        try
        {
            var configFile = manager.ConfigFilePath;
            var jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFile, jsonString);
        }
        catch (Exception ex)
        {
            Error($"保存管理器 {manager.GetType().Name} 配置失败", ex);
        }
    }
}
