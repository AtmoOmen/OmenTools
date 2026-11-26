using System.Runtime.CompilerServices;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace OmenTools.Extensions;

public static unsafe class RaptureAtkModuleExtension
{
    extension(scoped ref RaptureAtkModule module)
    {
        /// <summary>
        ///     根据界面名称获取对应的原生界面工厂信息。
        /// </summary>
        /// <param name="addonName">要查找的原生界面名称，忽略大小写。</param>
        /// <returns>界面工厂信息地址；名称不存在或索引无效时返回空指针。</returns>
        public RaptureAtkModule.AddonFactoryInfo* GetAddonFactoryInfo(string addonName)
        {
            var addonIndex = module.AddonNames
                                   .FindIndex(name => string.Equals(name.ExtractText(), addonName, StringComparison.OrdinalIgnoreCase));
            if (addonIndex < 0)
                return null;

            var factories = module.AddonFactories;
            if (addonIndex >= factories.Length)
                return null;

            return (RaptureAtkModule.AddonFactoryInfo*)Unsafe.AsPointer(ref factories[addonIndex]);
        }
    }
}
