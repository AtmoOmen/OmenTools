using Dalamud.Plugin;

namespace OmenTools.Extensions;

public static class DalamudPluginInterfaceExtension
{
    extension(IDalamudPluginInterface pi)
    {
        public bool IsPluginEnabled(string internalName, Predicate<IExposedPlugin>? predicate = null) =>
            pi.InstalledPlugins.Any
            (x => x.InternalName == internalName &&
                  x.IsLoaded                     &&
                  (predicate == null || predicate(x))
            );
    }
}
