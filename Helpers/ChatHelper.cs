using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using OmenTools.Infos;

namespace OmenTools.Helpers;

public sealed unsafe class ChatHelper
{
    private static readonly Lazy<ChatHelper> LazyInstance = new(() => new ChatHelper());
    private delegate void ProcessChatBoxDelegate(UIModule* module, Utf8String* message, nint a3, byte a4);

    private static readonly CompSig ProcessChatBoxSig = new("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B F2 48 8B F9 45 84 C9");
    private static readonly ProcessChatBoxDelegate? ProcessChatBox;

    static ChatHelper()
    {
        ProcessChatBox ??=
            Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(ProcessChatBoxSig.ScanText());
    }

    public static ChatHelper Instance => LazyInstance.Value;

    public void SendMessageUnsafe(Utf8String* message)
        => ProcessChatBox!(UIModule.Instance(), message, nint.Zero, 0);
    
    public void SendMessageUnsafe(ReadOnlySpan<byte> message)
    {
        fixed (byte* pMessage = message)
        {
            var mes = Utf8String.FromSequence(pMessage);
            ProcessChatBox!(UIModule.Instance(), mes, nint.Zero, 0);
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
        uText->SanitizeString((AllowedEntities)0x27F, (Utf8String*)nint.Zero);
        var sanitised = uText->ToString();
        uText->Dtor(true);
        return sanitised;
    }
}
