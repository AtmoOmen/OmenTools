using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using InteropGenerator.Runtime;

namespace OmenTools.Infos;

// TODO: 等 FFCS 合并
[StructLayout(LayoutKind.Explicit, Size = 39792)]
public unsafe struct InfoProxyLetterTemp
{
    public static InfoProxyLetterTemp* Instance() => 
        (InfoProxyLetterTemp*)InfoModule.Instance()->GetInfoProxyById(InfoProxyId.Letter);
    
    [FieldOffset(0)]  public InfoProxyInterface Interface;
    [FieldOffset(32)] public uint               NumOfDeniedLetters;
    [FieldOffset(36)] public ushort             NumAttachments;
    [FieldOffset(38)] public byte               NumNewLetters;
    [FieldOffset(39)] public byte               NumLettersFromFriends;     // 100 max
    [FieldOffset(40)] public byte               NumLettersFromPurchases;   // 20 max
    [FieldOffset(41)] public byte               NumLettersFromGameMasters; // 10 max
    [FieldOffset(42)] public bool               HasLettersFromGameMasters;
    [FieldOffset(43)] public bool               HasLettersFromSupportDesk;

    [FieldOffset(48)] private fixed byte letters[130 * 232];
    public Span<Letter> Letters
    {
        get
        {
            fixed (byte* ptr = letters)
            {
                return new Span<Letter>((Letter*)ptr, 130);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 232)]
    public struct Letter
    {
        [FieldOffset(0)] public long SenderContentID; // 0xFFFFFFFF for Store
        [FieldOffset(8)] public int  Timestamp;

        [FieldOffset(12)] private fixed byte attachments[5 * 8];
        public Span<ItemAttachment> Attachments
        {
            get
            {
                fixed (byte* ptr = attachments)
                {
                    return new Span<ItemAttachment>((ItemAttachment*)ptr, 5);
                }
            }
        }

        [FieldOffset(116)] public uint Gil;
        
        [MarshalAs(UnmanagedType.U1)]
        [FieldOffset(120)] public bool Read;

        [FieldOffset(135)] private fixed byte sender[32];
        public CStringPointer Sender
        {
            get
            {
                fixed (byte* ptr = sender)
                {
                    return ptr;
                }
            }
        }

        [FieldOffset(167)] private fixed byte messagePreview[64];
        public CStringPointer MessagePreview
        {
            get
            {
                fixed (byte* ptr = messagePreview)
                {
                    return ptr;
                }
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct ItemAttachment
        {
            [FieldOffset(0)] public uint ItemID;
            [FieldOffset(4)] public uint Count;
        }
    }
}
