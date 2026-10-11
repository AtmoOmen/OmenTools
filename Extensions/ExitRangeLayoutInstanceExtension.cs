using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Layer;

namespace OmenTools.Extensions;

public static unsafe class ExitRangeLayoutInstanceExtension
{
    extension
    (
        scoped ref ExitRangeLayoutInstance instance
    )
    {
        public PopRangeLayoutInstance* GetReturnInstance()
        {
            fixed (ExitRangeLayoutInstance* exit = &instance)
            {
                return (PopRangeLayoutInstance*)LayoutWorld.GetLayoutInstanceStatic
                (
                    InstanceType.PopRange,
                    exit->ReturnInstanceId
                );
            }
        }
    }
}
