using System.Collections;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using LuminaStatus = Lumina.Excel.Sheets.Status;
using CSStatus = FFXIVClientStructs.FFXIV.Client.Game.Status;

namespace OmenTools.Service;

public sealed unsafe class StatusList(nint address) : IReadOnlyCollection<Status>, ICollection
{
    public StatusList(void* pointer) : this((nint)pointer) { }
    
    public Status? this[int index]
    {
        get
        {
            if (index < 0 || index > this.Length)
                return null;

            return new Status(Address);
        }
    }

    public nint Address { get; } = address;
    public int  Length  => Struct->NumValidStatuses;

    public nint GetStatusAddress(int index)
    {
        if (index < 0 || index >= this.Length)
            return nint.Zero;

        return (nint)Unsafe.AsPointer(ref this.Struct->Status[index]);
    }
    
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
    
    private StatusManager* Struct => (StatusManager*)this.Address;
}

public unsafe class Status(nint address)
{
    public nint                 Address       { get; } = address;
    public uint                 StatusId      => this.Struct->StatusId;
    public RowRef<LuminaStatus> GameData      => LuminaCreateRef<LuminaStatus>(this.Struct->StatusId);
    public ushort               Param         => this.Struct->Param;
    public float                RemainingTime => this.Struct->RemainingTime;
    public uint                 SourceId      => this.Struct->SourceId;
    public IGameObject?         SourceObject  => DService.ObjectTable.SearchById(this.SourceId);

    private CSStatus* Struct => (CSStatus*)this.Address;
}
