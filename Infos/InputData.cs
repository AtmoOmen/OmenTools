using FFXIVClientStructs.FFXIV.Client.UI;
using System.Runtime.InteropServices;

namespace OmenTools.Infos;

public sealed unsafe class InputData : IDisposable
{
    private nint Bytes;
    private bool disposedValue;

    private InputData()
    {
        Bytes = Marshal.AllocHGlobal(0x40);
        Data = (void**)Bytes;
        if (Data == null)
            throw new ArgumentNullException();

        Data[0] = null;
        Data[1] = null;
        Data[2] = null;
        Data[3] = null;
        Data[4] = null;
        Data[5] = null;
        Data[6] = null;
        Data[7] = null;
    }

    public void** Data { get; }

    public static InputData Empty() => new();

    public static InputData ForPopupMenu(PopupMenu* popupMenu, ushort index)
    {
        var data = new InputData();
        data.Data[0] = popupMenu->List->ItemRendererList[index].AtkComponentListItemRenderer;
        data.Data[2] = (void*)(index | ((ulong)index << 48));
        return data;
    }

    private void Dispose(bool disposing)
    {
        if(disposedValue) return;
        if (disposing) 
        { }

        Marshal.FreeHGlobal(Bytes);
        disposedValue = true;
    }

    ~InputData() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
