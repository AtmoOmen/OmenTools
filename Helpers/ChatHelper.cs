using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace OmenTools.Helpers;

public sealed unsafe class ChatHelper
{
    private static readonly Lazy<ChatHelper> LazyInstance = new(() => new ChatHelper());
    private delegate void ProcessChatBoxDelegate(UIModule* module, Utf8String* message, nint a3, byte a4);

    private static readonly ProcessChatBoxDelegate? ProcessChatBox;

    static ChatHelper()
    {
        ProcessChatBox ??=
            Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(
                DService.SigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9"));
    }

    public static ChatHelper Instance => LazyInstance.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SendMessageUnsafe(ReadOnlySpan<byte> message)
    {
        fixed (byte* pMessage = message)
        {
            var mes = Utf8String.FromSequence(pMessage);
            ProcessChatBox(UIModule.Instance(), mes, IntPtr.Zero, 0);
            mes->Dtor(true);
        }
    }

    public void SendMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            DService.Log.Error("待发送消息为空");
            return;
        }

        if (message.Length != SanitiseText(message).Length)
        {
            DService.Log.Error($"待发送消息中包含非法字符:\n{message}");
            return;
        }

        ReadOnlySpan<byte> bytes = Encoding.UTF8.GetBytes(message);
        if (bytes.Length > 500)
        {
            DService.Log.Error($"待发送消息不能长于 500 字节:\n{message}");
            return;
        }

        SendMessageUnsafe(bytes);
    }

    private static string SanitiseText(string text)
    {
        var uText = Utf8String.FromString(text);
        uText->SanitizeString(0x27F, (Utf8String*)nint.Zero);
        var sanitised = uText->ToString();
        uText->Dtor(true);
        return sanitised;
    }
}
