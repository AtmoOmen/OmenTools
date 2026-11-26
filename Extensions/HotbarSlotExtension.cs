using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace OmenTools.Extensions;

public static class HotbarSlotExtension
{
    extension(scoped in RaptureHotbarModule.HotbarSlot slot)
    {
        /// <summary>
        ///     获取槽位是否包含有效的显示类型、指令类型和指令编号。
        /// </summary>
        public bool IsValid =>
            slot.ApparentSlotType is not RaptureHotbarModule.HotbarSlotType.Empty         &&
            slot.CommandType is not RaptureHotbarModule.HotbarSlotType.Empty              &&
            slot.OriginalApparentSlotType is not RaptureHotbarModule.HotbarSlotType.Empty &&
            slot.CommandId != uint.MaxValue;
    }
}
