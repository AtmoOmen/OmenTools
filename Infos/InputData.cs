using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace OmenTools.Infos;

public sealed unsafe class InputData : IDisposable
{
    private readonly InputDataSafeHandle handle;

    private bool disposedValue;

    private InputData()
    {
        handle = new InputDataSafeHandle();
        Data   = (void**)handle.DangerousGetHandle();
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
        if (disposedValue) return;
        if (disposing) 
            handle.Dispose();

        disposedValue = true;
    }

    public void Dispose() => 
        Dispose(true);

    private sealed class InputDataSafeHandle : SafeHandle
    {
        public InputDataSafeHandle() : base(nint.Zero, true) => 
            SetHandle(Marshal.AllocHGlobal(0x40));

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
