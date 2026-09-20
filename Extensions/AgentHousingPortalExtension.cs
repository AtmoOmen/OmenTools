using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using OmenTools.Interop.Game.Models;

namespace OmenTools.Extensions;

public static unsafe class AgentHousingPortalExtension
{
    private static readonly CompSig AgentHousingPortalNotifyCallbackSig =
        new("48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 83 79 28 00 41 8B F0 8B EA 48 8B F9");
    private delegate void NotifyCallbackDelegate
    (
        AgentHousingPortal* agent,
        int                 action,
        int                 argument
    );
    private static readonly NotifyCallbackDelegate NotifyCallback =
        AgentHousingPortalNotifyCallbackSig.GetDelegate<NotifyCallbackDelegate>();

    extension
    (
        scoped ref AgentHousingPortal agent
    )
    {
        /// <summary>
        ///     传送到当前住宅区的指定区，需要先交互。
        /// </summary>
        public void TeleportToWard
        (
            int wardIndex
        ) =>
            NotifyCallback((AgentHousingPortal*)Unsafe.AsPointer(ref agent), 3, wardIndex);
    }
}
