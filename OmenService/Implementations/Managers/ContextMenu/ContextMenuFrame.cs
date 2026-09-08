using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.OmenService;

internal sealed unsafe class ContextMenuFrame
(
    int valueCount
) : IDisposable
{
    private int references = 1;

    public AtkValue* Values { get; private set; } = (AtkValue*)NativeMemory.AllocZeroed((nuint)valueCount, (nuint)sizeof(AtkValue));

    public int ValueCount { get; } = valueCount;

    public uint AddonNameID { get; init; }

    public ushort AddonID { get; set; }

    public AgentInterface* Agent { get; init; }

    public ulong EventKind { get; init; }

    public ushort OwnerAddonID { get; init; }

    public int DepthLayer { get; init; }

    public bool IsSubmenu { get; init; }

    public int SelectedIndex { get; set; }

    public short X { get; set; }

    public short Y { get; set; }

    public ContextMenuItem[] Items { get; init; } = [];

    public int[] CallbackIDs { get; init; } = [];

    public void Retain() => references++;

    public void CopyValue(int index, AtkValue* source)
    {
        if ((AtkValueType)((int)source->Type & (int)AtkValueType.TypeMask) is AtkValueType.String or AtkValueType.ConstString)
            Values[index].SetManagedString(source->String);
        else
            Values[index].Copy(source);
    }

    public void Dispose()
    {
        if (Values is null)
            return;
        if (--references > 0)
            return;

        for (var i = 0; i < ValueCount; i++)
            Values[i].Dtor();

        NativeMemory.Free(Values);
        Values = null;
    }
}
