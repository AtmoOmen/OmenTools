using System.Runtime.InteropServices;

namespace OmenTools.Infos;

public sealed unsafe class EventData : IDisposable
{
    private readonly nint Bytes;
    private          bool disposedValue;

    private EventData()
    {
        Bytes = Marshal.AllocHGlobal(0x18);
        Data  = (void**)Bytes;
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
        { }

        Marshal.FreeHGlobal(Bytes);
        disposedValue = true;
    }

    ~EventData() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
