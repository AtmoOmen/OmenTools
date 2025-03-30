using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lumina.Excel;

namespace OmenTools.Service;

public sealed unsafe partial class StatusList
{
    internal StatusList(nint address) => this.Address = address;
    
    internal StatusList(void* pointer) : this((nint)pointer) { }

    public nint Address { get; }

    public int Length => Struct->NumValidStatuses;

    private static int StatusSize { get; } = Marshal.SizeOf<FFXIVClientStructs.FFXIV.Client.Game.Status>();

    private FFXIVClientStructs.FFXIV.Client.Game.StatusManager* Struct => 
        (FFXIVClientStructs.FFXIV.Client.Game.StatusManager*)this.Address;

    public Status? this[int index]
    {
        get
        {
            if (index < 0 || index > this.Length)
                return null;

            var addr = this.GetStatusAddress(index);
            return CreateStatusReference(addr);
        }
    }

    public static StatusList? CreateStatusListReference(nint statusManager) => 
        DService.ClientState.LocalContentId == 0 ? null : statusManager == nint.Zero ? null : new StatusList(statusManager);

    public static Status? CreateStatusReference(nint address) => 
        DService.ClientState.LocalContentId == 0 ? null : address == nint.Zero ? null : new Status(address);

    public nint GetStatusAddress(int index)
    {
        if (index < 0 || index >= this.Length)
            return nint.Zero;

        return (nint)Unsafe.AsPointer(ref this.Struct->Status[index]);
    }
}

public sealed partial class StatusList : IReadOnlyCollection<Status>, ICollection
{
    int IReadOnlyCollection<Status>.Count => this.Length;

    int ICollection.Count => this.Length;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    public IEnumerator<Status> GetEnumerator()
    {
        for (var i = 0; i < this.Length; i++)
        {
            var status = this[i];

            if (status == null || status.StatusId == 0)
                continue;

            yield return status;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    void ICollection.CopyTo(Array array, int index)
    {
        for (var i = 0; i < this.Length; i++)
        {
            array.SetValue(this[i], index);
            index++;
        }
    }
}

public unsafe class Status
{
    internal Status(nint address) => this.Address = address;

    public nint Address { get; }

    public uint StatusId => this.Struct->StatusId;

    public RowRef<Lumina.Excel.Sheets.Status> GameData => CreateRef<Lumina.Excel.Sheets.Status>(this.Struct->StatusId);

    public ushort Param => this.Struct->Param;

    [Obsolete($"Replaced with {nameof(Param)}", true)]
    public byte StackCount => (byte)this.Struct->Param;

    public float RemainingTime => this.Struct->RemainingTime;

    public uint SourceId => this.Struct->SourceId;
    
    public IGameObject? SourceObject => DService.ObjectTable.SearchById(this.SourceId);

    private FFXIVClientStructs.FFXIV.Client.Game.Status* Struct => (FFXIVClientStructs.FFXIV.Client.Game.Status*)this.Address;
}
