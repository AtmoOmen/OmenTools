using OmenTools.OmenService.Abstractions;

namespace OmenTools.Extensions;

public static class OmenServiceConfigExtension
{
    extension<T>(T config) where T : OmenServiceConfig
    {
        public static T? Load(OmenServiceBase instance) =>
            instance.LoadConfig<T>();

        public void Save(OmenServiceBase instance) =>
            instance.SaveConfig(config);
    }
}
