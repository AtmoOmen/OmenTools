using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.Interop;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace OmenTools.Extensions;

public static unsafe class LogMessageExtension
{
    extension(scoped ref LogMessageQueueItem item)
    {
        public ReadOnlySeString ToReadOnlySeString()
        {
            fixed (LogMessageQueueItem* ptr = &item)
            {
                if (ptr == null || item.LogMessageId == 0)
                    return new();

                var logModule = RaptureLogModule.Instance();

                if (item.SourceKind != EntityRelationKind.None)
                {
                    var name = item.SourceName.GetPointer(0);

                    if (item.SourceIsPlayer)
                    {
                        var str = logModule->TempParseMessage.GetPointer(8);
                        logModule->FormatPlayerLink(name, str, null, 0, (byte)item.SourceKind != 1 /* LocalPlayer */, item.SourceHomeWorld, false, null, false);

                        if (item.SourceHomeWorld != 0 && item.SourceHomeWorld != AgentLobby.Instance()->LobbyData.HomeWorldId)
                        {
                            var crossWorldSymbol = logModule->RaptureTextModule->UnkStrings0.GetPointer(3);
                            if (!crossWorldSymbol->StringPtr.HasValue)
                                logModule->RaptureTextModule->ProcessMacroCode(crossWorldSymbol, "<icon(88)>\0"u8);
                            str->Append(crossWorldSymbol);
                            if (logModule->UIModule->GetWorldHelper()->AllWorlds.TryGetValuePointer(item.SourceHomeWorld, out var world))
                                str->ConcatCStr(world->Name);
                        }

                        name = str->StringPtr;
                    }

                    logModule->RaptureTextModule->SetGlobalTempEntity1(name, item.SourceSex, item.SourceObjStrId);
                }

                if (item.TargetKind != EntityRelationKind.None)
                {
                    var name = item.TargetName.GetPointer(0);

                    if (item.TargetIsPlayer)
                    {
                        var str = logModule->TempParseMessage.GetPointer(0);
                        logModule->FormatPlayerLink(name, str, null, 0, (byte)item.TargetKind != 1 /* LocalPlayer */, item.TargetHomeWorld, false, null, false);

                        if (item.TargetHomeWorld != 0 && item.TargetHomeWorld != AgentLobby.Instance()->LobbyData.HomeWorldId)
                        {
                            var crossWorldSymbol = logModule->RaptureTextModule->UnkStrings0.GetPointer(3);
                            if (!crossWorldSymbol->StringPtr.HasValue)
                                logModule->RaptureTextModule->ProcessMacroCode(crossWorldSymbol, "<icon(88)>\0"u8);
                            str->Append(crossWorldSymbol);
                            if (logModule->UIModule->GetWorldHelper()->AllWorlds.TryGetValuePointer(item.TargetHomeWorld, out var world))
                                str->ConcatCStr(world->Name);
                        }

                        name = str->StringPtr;
                    }

                    logModule->RaptureTextModule->SetGlobalTempEntity2(name, item.TargetSex, item.TargetObjStrId);
                }

                using var rssb = new RentedSeStringBuilder();

                using var utf8 = new Utf8String();
                logModule->RaptureTextModule->FormatString
                (
                    rssb.Builder.Append(item.LogMessageId.ToLuminaRowRef<LogMessage>().Value.Text).GetViewAsSpan(),
                    &ptr->Parameters,
                    &utf8
                );

                return new(utf8.AsSpan());
            }
        }
    }

}
