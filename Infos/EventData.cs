using System.Runtime.InteropServices;

namespace OmenTools.Infos;

public sealed unsafe class EventData : IDisposable
{
    private readonly EventDataSafeHandle handle;

    private bool disposedValue;

    private EventData()
    {
        handle = new EventDataSafeHandle();
        Data   = (void**)handle.DangerousGetHandle();
        if (Data == null)
            throw new ArgumentNullException();

        Data[0] = null;
        Data[1] = null;
        Data[2] = null;
    }

    public void** Data { get; }

    public static EventData ForNormalTarget(void* target, void* listener)
    {
        var data = new EventData();
        data.Data[1] = target;
        data.Data[2] = listener;
        return data;
    }

    private void Dispose(bool disposing)
    {
        if (disposedValue) return;
        if (disposing)
            handle.Dispose();

        disposedValue = true;
    }

    public void Dispose() => 
        Dispose(true);

    private sealed class EventDataSafeHandle : SafeHandle
    {
        public EventDataSafeHandle() : base(nint.Zero, true) =>
            SetHandle(Marshal.AllocHGlobal(0x18));

        public override bool IsInvalid =>
            handle == nint.Zero;

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
                Marshal.FreeHGlobal(handle);

            return true;
        }
    }
}
