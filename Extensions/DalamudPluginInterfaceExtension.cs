using Dalamud.Plugin;

namespace OmenTools.Extensions;

public static class DalamudPluginInterfaceExtension
{
    extension(IDalamudPluginInterface pi)
    {
        public bool IsPluginEnabled(string internalName, Version? minVersion = null) =>
            pi.InstalledPlugins.Any(x => x.InternalName == internalName && x.IsLoaded && (minVersion == null || x.Version >= minVersion));
    }
}
