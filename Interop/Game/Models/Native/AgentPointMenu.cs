using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Interop.Game.Models.Native;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct AgentPointMenu
{
    [FieldOffset(0)]
    public AgentInterface AgentInterface;

    [FieldOffset(36)]
    public PendingResultFlag PendingResultFlags;

    [FieldOffset(40)]
    public PointMenuContext* Context;

    [FieldOffset(48)]
    public nint CompletionTreeRoot;

    [FieldOffset(56)]
    public int Phase;

    [FieldOffset(60)]
    public int SelectedIndex;

    public readonly int EntryCount =>
        Context != null ? (int)(((byte*)Context->pEntriesEnd - (byte*)Context->pEntries) / sizeof(PointMenuEntry)) : 0;

    public readonly int FindFirstUncompletedEntry()
    {
        if (CompletionTreeRoot == 0) return 0;
        if (Context            == null) return -1;

        var rootNode = *(CompletionTreeNode**)CompletionTreeRoot;
        var key      = Context->CompletionKey;

        var current = rootNode->Right;
        var parent  = rootNode;

        while (current->IsSentinel == 0)
        {
            if (current->Key >= key)
            {
                parent  = current;
                current = current->Left;
            }
            else current = current->Right;
        }

        if (parent->IsSentinel != 0 || key < parent->Key || parent == rootNode)
            return -1;

        var bitfield = parent->CompletedBitfield;
        var count    = EntryCount;

        for (var i = 0; i < count; i++)
            if ((bitfield & 1 << i) == 0)
                return i;

        return -1;
    }

    public static AgentPointMenu* Instance() =>
        (AgentPointMenu*)AgentModule.Instance()->GetAgentByInternalId(AgentId.PointMenu);
    
    [Flags]
    public enum PendingResultFlag : byte
    {
        None             = 0,
        HasPendingResult = 2
    }
}

[StructLayout(LayoutKind.Explicit, Size = 36)]
public unsafe struct CompletionTreeNode
{
    [FieldOffset(0)]
    public CompletionTreeNode* Left;

    [FieldOffset(8)]
    public CompletionTreeNode* Right;

    [FieldOffset(25)]
    public byte IsSentinel;

    [FieldOffset(28)]
    public int Key;

    [FieldOffset(32)]
    public int CompletedBitfield;
}

[StructLayout(LayoutKind.Explicit, Size = 320)]
public unsafe struct PointMenuContext
{
    [FieldOffset(0)]
    public AgentPointMenu* Agent;

    [FieldOffset(8)]
    public nint ChoiceSheet;

    [FieldOffset(16)]
    public nint ChoiceSheetWaiter;

    [FieldOffset(288)]
    public PointMenuEntry* pEntries;

    [FieldOffset(296)]
    public PointMenuEntry* pEntriesEnd;

    [FieldOffset(304)]
    public PointMenuEntry* pEntriesCapacity;

    [FieldOffset(312)]
    public int CompletionKey;

    [FieldOffset(316)]
    public byte IsLoaded;
}

[StructLayout(LayoutKind.Explicit, Size = 136)]
public struct PointMenuEntry
{
    [FieldOffset(0)]
    public float X;

    [FieldOffset(4)]
    public float Y;

    [FieldOffset(8)]
    public float ClickAreaX;

    [FieldOffset(12)]
    public float ClickAreaY;

    [FieldOffset(16)]
    public float ClickAreaWidth;

    [FieldOffset(20)]
    public float ClickAreaHeight;

    [FieldOffset(28)]
    public byte NavUp;

    [FieldOffset(29)]
    public byte NavDown;

    [FieldOffset(30)]
    public byte NavLeft;

    [FieldOffset(31)]
    public byte NavRight;

    [FieldOffset(32)]
    public unsafe fixed byte Text[104];
}
