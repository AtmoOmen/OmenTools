using System.Reflection;
using System.Runtime.Loader;
using Dalamud.Plugin;
using Task = System.Threading.Tasks.Task;

#nullable disable

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    private const BindingFlags BindingAllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private const BindingFlags BindingStaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    private const BindingFlags BindingInstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public static object GetPluginManager() =>
        DService.PI.GetType().Assembly.
                 GetType("Dalamud.Service`1", true)
                 ?.MakeGenericType(DService.PI.GetType().Assembly.GetType("Dalamud.Plugin.Internal.PluginManager", true)!).
                 GetMethod("Get")
                 ?.Invoke(null, BindingFlags.Default, null, [], null);
    
    public static async Task<List<object>> GetPluginMaster(string masterURL)
    {
        List<object> plugins = null;

        try
        {
            var happyHttpClient = GetService("Dalamud.Networking.Http.HappyHttpClient");
            var pluginRepository = Activator.CreateInstance(
                DService.PI.GetType().Assembly
                    .GetType("Dalamud.Plugin.Internal.Types.PluginRepository")!,
                happyHttpClient, masterURL, true);
            await pluginRepository?.Call<Task>("ReloadPluginMasterAsync", [])!;

            var pluginMaster = pluginRepository!.GetType()
                .GetProperty("PluginMaster")!
                .GetValue(pluginRepository) as System.Collections.IEnumerable;

            plugins = pluginMaster?.Cast<object>().ToList();
        }
        catch (Exception)
        {
            return null;
        }

        return plugins;
    }

    public static object GetService(string serviceFullName) =>
        DService.PI.GetType().Assembly.
                 GetType("Dalamud.Service`1", true)
                 ?.MakeGenericType(DService.PI.GetType().Assembly.GetType(serviceFullName, true)!).
                 GetMethod("Get")
                 ?.Invoke(null, BindingFlags.Default, null, [], null);

    public static bool TryGetLocalPlugin(IDalamudPlugin instance, out object localPlugin, out AssemblyLoadContext context, out Type type)
    {
        type        = null;
        context     = null;
        localPlugin = null;
        
        try
        {
            var pluginManager = GetPluginManager();
            var installedPlugins = (System.Collections.IList)pluginManager.GetType().GetProperty("InstalledPlugins")?.GetValue(pluginManager);
            if (installedPlugins == null) return false;
            
            foreach (var t in installedPlugins)
            {
                type = t.GetType().Name == "LocalDevPlugin" ? t.GetType().BaseType : t.GetType();
                if (type == null) continue;
                
                var plugin = (IDalamudPlugin)type
                                             .GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance)
                                             ?.GetValue(t);
                if (plugin == instance)
                {
                    localPlugin = t;
                    context = type.GetField("loader", BindingAllFlags)
                                  ?.GetValue(t)
                                  ?.GetFoP<AssemblyLoadContext>("context");
                    return true;
                }
            }

            return false;
        }
        catch
        {
            // ignored
        }

        return false;
    }

    public static bool HasRepo(string repoURL)
    {
        var conf = GetService("Dalamud.Configuration.Internal.DalamudConfiguration");
        var repoList = (System.Collections.IEnumerable)conf.GetFoP("ThirdRepoList");
        
        if(repoList != null)
            foreach(var r in repoList)
                if((string)r.GetFoP("Url") == repoURL)
                    return true;
        
        return false;
    }

    public static void AddRepo(string repoURL, bool enabled)
    {
        var conf = GetService("Dalamud.Configuration.Internal.DalamudConfiguration");
        var repoList = (System.Collections.IEnumerable)conf.GetFoP("ThirdRepoList");
        if(repoList != null)
            foreach(var r in repoList)
                if((string)r.GetFoP("Url") == repoURL)
                    return;
        
        var instance = Activator.CreateInstance(DService.PI.GetType().Assembly.GetType("Dalamud.Configuration.ThirdPartyRepoSettings")!);
        if (instance == null) return;
        
        instance.SetFoP("Url", repoURL);
        instance.SetFoP("IsEnabled", enabled);
        conf.GetFoP<System.Collections.IList>("ThirdRepoList").Add(instance!);
    }

    public static async Task<bool> AddPlugin(string masterURL, string pluginInternalName)
    {
        var plugins = await GetPluginMaster(masterURL);
        if (plugins == null || plugins.Count == 0) return false;
        
        var pluginManifest = plugins.FirstOrDefault(x => (string)x.GetFoP("InternalName") == pluginInternalName);
        if (pluginManifest == null) return false;

        object pm = null;
        var error = string.Empty;
        try
        {
            pm = GetPluginManager();
        }
        catch (Exception e)
        {
            error = e.Message;
        }
        
        if (pm == null || !string.IsNullOrWhiteSpace(error))
            return false;

        try
        {
            if (!HasRepo(masterURL)) AddRepo(masterURL, true);
            ReloadPluginMasters();

            var installCall = pm.Call<Task>("InstallPluginAsync", [pluginManifest, false, PluginLoadReason.Installer, null]);
            await installCall;

            var localPlugin = installCall.GetFoP("Result");
            if ((bool?)localPlugin?.GetFoP("IsLoaded") ?? false) 
                return true;
        }
        catch
        {
            // ignored
        }
        
        return false;
    }
    
    public static void ReloadPluginMasters()
    {
        var mgr = GetService("Dalamud.Plugin.Internal.PluginManager");
        var pluginReload = mgr?.GetType().GetMethod("SetPluginReposFromConfigAsync", BindingFlags.Instance | BindingFlags.Public);
        pluginReload?.Invoke(mgr, [true]);
    }

    public static void SaveDalamudConfig()
    {
        var conf = GetService("Dalamud.Configuration.Internal.DalamudConfiguration");
        var configSave = conf?.GetType().GetMethod("QueueSave", BindingFlags.Instance | BindingFlags.Public);
        configSave?.Invoke(conf, null);
    }
}
