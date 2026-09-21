using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Interop.Game.Models.Native;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct AgentFreeCompanyProfile
{
    private static readonly CompSig ShowForContentIDSig =
        new("48 89 5C 24 ?? 57 48 83 EC ?? 48 8B 01 48 8B FA 48 8B D9 FF 50 ?? 84 C0 74 ?? 48 8B 03 48 8B CB FF 50 ?? 48 89 7B");
    private delegate void ShowForContentIDDelegate
    (
        AgentFreeCompanyProfile* agent,
        ulong                    contentID
    );
    private static readonly ShowForContentIDDelegate ShowForContentID = ShowForContentIDSig.GetDelegate<ShowForContentIDDelegate>();

    public void ShowProfile
    (
        ulong contentID
    )
    {
        fixed (AgentFreeCompanyProfile* instance = &this)
            ShowForContentID(instance, contentID);
    }

    public static AgentFreeCompanyProfile* Instance() =>
        (AgentFreeCompanyProfile*)AgentModule.Instance()->GetAgentByInternalId(AgentId.FreeCompanyProfile);
}
