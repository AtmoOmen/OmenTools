using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using LuminaStatus = Lumina.Excel.Sheets.Status;
using CSStatus = FFXIVClientStructs.FFXIV.Client.Game.Status;

namespace OmenTools.Service;

public sealed unsafe partial class StatusList
{
    internal StatusList(nint address) => 
        Address = address;
    
    internal StatusList(void* pointer)
        : this((nint)pointer) { }
    
    public nint Address { get; }
    
    public int Length => Struct->NumValidStatuses;

    private static int StatusSize { get; } = Marshal.SizeOf<CSStatus>();

    private StatusManager* Struct => (StatusManager*)Address;
    
    public IStatus? this[int index]
    {
        get
        {
            if (index < 0 || index > Length)
                return null;

            var addr = GetStatusAddress(index);
            return CreateStatusReference(addr);
        }
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

    public bool TryGetStatus(uint statusID, out IStatus? status, out int index)
    {
        status = null;
        index  = -1;

        for (var i = 0; i < this.Length; i++)
        {
            var addr = GetStatusAddress(i);
            if (addr == nint.Zero) continue;
            
            var currentStatus = (CSStatus*)addr;
            if (currentStatus->StatusId != statusID)
                continue;

            status = CreateStatusReference(addr);
            index  = i;
            return true;
        }

        return false;
    }
    
    public static StatusList? CreateStatusListReference(nint address)
    {
        if (address == nint.Zero)
            return null;
        
        if (LocalPlayerState.ContentID == 0)
            return null;

        if (address == 0)
            return null;

        return new StatusList(address);
    }
    
    public static IStatus? CreateStatusReference(nint address)
    {
        if (address == nint.Zero)
            return null;

        if (address == 0)
            return null;

        return new Status((CSStatus*)address);
    }
    
    public nint GetStatusAddress(int index)
    {
        if (index < 0 || index >= Length)
            return 0;

        return (nint)Unsafe.AsPointer(ref Struct->Status[index]);
    }
}

public sealed partial class StatusList : IReadOnlyCollection<IStatus>, ICollection
{
    int IReadOnlyCollection<IStatus>.Count => Length;
    
    int ICollection.Count => Length;
    
    bool ICollection.IsSynchronized => false;
    
    object ICollection.SyncRoot => this;
    
    public IEnumerator<IStatus> GetEnumerator() => 
        new Enumerator(this);
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    void ICollection.CopyTo(Array array, int index)
    {
        for (var i = 0; i < Length; i++)
        {
            array.SetValue(this[i], index);
            index++;
        }
    }

    private struct Enumerator(StatusList statusList) : IEnumerator<IStatus>
    {
        private int index = -1;

        public IStatus Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            while (++index < statusList.Length)
            {
                var status = statusList[index];
                if (status != null && status.StatusID != 0)
                {
                    Current = status;
                    return true;
                }
            }

            Current = null;
            return false;
        }

        public void Reset() => 
            index = -1;

        public void Dispose() { }
    }
}

public interface IStatus : IEquatable<IStatus>
{
    nint Address { get; }

    uint StatusID { get; }
    
    RowRef<LuminaStatus> GameData { get; }
    
    ushort Param { get; }
    
    float RemainingTime { get; }
    
    uint SourceID { get; }
    
    IGameObject? SourceObject { get; }

    static unsafe IStatus Create(nint address) => new Status((CSStatus*)address);
}

internal readonly unsafe struct Status(CSStatus* ptr) : IStatus
{
    public nint Address => (nint)ptr;
    
    public uint StatusID => ptr->StatusId;
    
    public RowRef<LuminaStatus> GameData => ptr->StatusId.ToLuminaRowRef<LuminaStatus>();
    
    public ushort Param => ptr->Param;
    
    public float RemainingTime => ptr->RemainingTime;
    
    public uint SourceID => ptr->SourceObject.ObjectId;
    
    public IGameObject? SourceObject => DService.Instance().ObjectTable.SearchByID(SourceID);
    
    public static bool operator ==(Status x, Status y) => x.Equals(y);

    public static bool operator !=(Status x, Status y) => !(x == y);
    
    public bool Equals(IStatus? other) => 
        StatusID == other.StatusID && SourceID == other.SourceID && Param == other.Param && RemainingTime == other.RemainingTime;
    
    public override bool Equals([NotNullWhen(true)] object? obj) => 
        obj is Status fate && Equals(fate);
    
    public override int GetHashCode() => 
        HashCode.Combine(StatusID, SourceID, Param, RemainingTime);
}
