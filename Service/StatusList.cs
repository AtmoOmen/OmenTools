using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using LuminaStatus = Lumina.Excel.Sheets.Status;
using CSStatus = FFXIVClientStructs.FFXIV.Client.Game.Status;

namespace OmenTools.Service;

public sealed unsafe class StatusList(nint address) : IReadOnlyCollection<Status>, ICollection
{
    public StatusList(void* pointer) : this((nint)pointer) { }

    public nint Address { get; } = address;

    public int Length => Struct->NumValidStatuses;

    private static int StatusSize { get; } = Marshal.SizeOf<CSStatus>();

    private StatusManager* Struct => (StatusManager*)this.Address;

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

    public static StatusList? CreateStatusListReference(nint address)
    {
        var clientState = DService.ClientState;

        if (clientState.LocalContentId == 0)
            return null;

        if (address == nint.Zero)
            return null;

        return new StatusList(address);
    }

    public static Status? CreateStatusReference(nint address)
    {
        var clientState = DService.ClientState;

        if (clientState.LocalContentId == 0)
            return null;

        if (address == nint.Zero)
            return null;

        return new Status(address);
    }

    public nint GetStatusAddress(int index)
    {
        if (index < 0 || index >= this.Length)
            return nint.Zero;

        return (nint)Unsafe.AsPointer(ref this.Struct->Status[index]);
    }

    public bool HasStatus(uint statusID)
    {
        for (var i = 0; i < this.Length; i++)
        {
            var addr = GetStatusAddress(i);
            if (addr == nint.Zero) continue;
            
            var status = (CSStatus*)addr;
            if (status->StatusId == statusID)
                return true;
        }
        
        return false;
    }

    public bool TryGetStatus(uint statusID, out Status? status, out int index)
    {
        status = null;
        index = -1;

        for (var i = 0; i < this.Length; i++)
        {
            var addr = GetStatusAddress(i);
            if (addr == nint.Zero) continue;
            
            var currentStatus = (CSStatus*)addr;
            if (currentStatus->StatusId != statusID)
                continue;

            status = CreateStatusReference(addr);
            index = i;
            return true;
        }

        return false;
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

            if (status == null || status.StatusID == 0)
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

public unsafe class Status(nint address)
{
    public nint                 Address       { get; } = address;
    public uint                 StatusID      => this.Struct->StatusId;
    public RowRef<LuminaStatus> GameData      => LuminaCreateRef<LuminaStatus>(this.Struct->StatusId);
    public ushort               Param         => this.Struct->Param;
    public float                RemainingTime => this.Struct->RemainingTime;
    public ulong                SourceID      => this.Struct->SourceObject;
    public IGameObject?         SourceObject  => DService.ObjectTable.SearchByID(this.SourceID);

    private CSStatus* Struct => (CSStatus*)this.Address;
}
