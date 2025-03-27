global using static OmenTools.Helpers.HelpersOm;
global using static OmenTools.Infos.InfosOm;

using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using OmenTools.Helpers;
using OmenTools.Service;

namespace OmenTools;

public class DService
{
    public static void Init(IDalamudPluginInterface pluginInterface)
    {
        PI = pluginInterface;
        UiBuilder       = pluginInterface.UiBuilder;
        
        pluginInterface.Create<DService>();
        ObjectTable = new ObjectTable();
        Targets = new TargetManager();
    }

    public static void Uninit()
    {
        TaskHelper.DisposeAll();
        MemoryPatch.DisposeAll();
        ThrottlerHelper.Uninit();
        ImageHelper.Uninit();
        TrayNotify.Uninit();
    }

    public static void InitTrayNotify(Icon icon, string multiMessagesReceived = "收到了 {0} 条新消息") 
        => TrayNotify.Init(icon, multiMessagesReceived);
    
    [PluginService] public static IAddonLifecycle      AddonLifecycle  { get; private set; } = null!;
    [PluginService] public static IAddonEventManager   AddonEvent      { get; private set; } = null!;
    [PluginService] public static IAetheryteList       AetheryteList   { get; private set; } = null!;
    [PluginService] public static IBuddyList           BuddyList       { get; private set; } = null!;
    [PluginService] public static IChatGui             Chat            { get; private set; } = null!;
    [PluginService] public static IClientState         ClientState     { get; private set; } = null!;
    [PluginService] public static ICommandManager      Command         { get; private set; } = null!;
    [PluginService] public static ICondition           Condition       { get; private set; } = null!;
    [PluginService] public static IContextMenu         ContextMenu     { get; private set; } = null!;
    [PluginService] public static IDataManager         Data            { get; private set; } = null!;
    [PluginService] public static IDtrBar              DtrBar          { get; private set; } = null!;
    [PluginService] public static IDutyState           DutyState       { get; private set; } = null!;
    [PluginService] public static IFateTable           Fate            { get; private set; } = null!;
    [PluginService] public static IFlyTextGui          FlyText         { get; private set; } = null!;
    [PluginService] public static IFramework           Framework       { get; private set; } = null!;
    [PluginService] public static IGameConfig          GameConfig      { get; private set; } = null!;
    [PluginService] public static IGameGui             Gui             { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider Hook            { get; private set; } = null!;
    [PluginService] public static IGameInventory       Inventory       { get; private set; } = null!;
    [PluginService] public static IGameLifecycle       Lifecycle       { get; private set; } = null!;
    [PluginService] public static IGameNetwork         Network         { get; private set; } = null!;
    [PluginService] public static IGamepadState        Gamepad         { get; private set; } = null!;
    [PluginService] public static IJobGauges           JobGauges       { get; private set; } = null!;
    [PluginService] public static IKeyState            KeyState        { get; private set; } = null!;
    [PluginService] public static IMarketBoard         MarketBoard     { get; private set; } = null!;
    [PluginService] public static INamePlateGui        NamePlateGui    { get; private set; } = null!;
    [PluginService] public static INotificationManager DNotice         { get; private set; } = null!;
    [PluginService] public static IPartyFinderGui      PartyFinder     { get; private set; } = null!;
    [PluginService] public static IPartyList           PartyList       { get; private set; } = null!;
    [PluginService] public static IPluginLog           Log             { get; private set; } = null!;
    [PluginService] public static ITextureProvider     Texture         { get; private set; } = null!;
    [PluginService] public static ITitleScreenMenu     TitleScreenMenu { get; private set; } = null!;
    [PluginService] public static IToastGui            Toast           { get; private set; } = null!;

    public static IDalamudPluginInterface PI          { get; private set; } = null!;
    public static IUiBuilder              UiBuilder   { get; private set; } = null!;
    public static IObjectTable            ObjectTable { get; private set; } = null!;
    public static ITargetManager          Targets     { get; private set; } = null!;
    public static SigScanner              SigScanner { get; private set; } = new();
}
