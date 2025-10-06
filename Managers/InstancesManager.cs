using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.Sheets;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class InstancesManager : OmenServiceBase
{
    public static unsafe bool   IsInstancedArea => UIState.Instance()->PublicInstance.IsInstancedArea();
    public static unsafe uint   CurrentInstance => UIState.Instance()->PublicInstance.InstanceId;
    public static unsafe string CurrentVersion  => Framework.Instance()->GameVersionString;
    public static unsafe int    InstanceAmount  => IsInstancedArea ? *InstanceAmountSig.GetStatic<int>() : 0;

    private static readonly CompSig InstanceAmountSig = new("4C 8D 0D ?? ?? ?? ?? 44 0F B7 41");
    
    private static TaskHelper? TaskHelper;
    
    private static Config ServiceConfig = null!;

    internal override void Init()
    {
        ServiceConfig = LoadConfig<Config>() ?? new();
        TaskHelper ??= new() { TimeLimitMS = 15_000 };

        ExecuteCommandManager.Register(OnPostExecuteCommand);
        EnqueueInstancesCountRetrieve(GameState.TerritoryType);
    }

    public static int GetInstancesCount(uint zoneID = 0)
    {
        if (zoneID == 0) 
            zoneID = GameState.TerritoryType;

        ServiceConfig.InstancesAmount.TryAdd(CurrentVersion, []);
        ServiceConfig.Save(DService.GetOmenService<InstancesManager>());

        return ServiceConfig.InstancesAmount[CurrentVersion].GetValueOrDefault(zoneID, 0);
    }

    public static unsafe bool TryGetAetheryteNearby(out uint eventID)
    {
        eventID = 0;

        foreach (var eve in EventFramework.Instance()->EventHandlerModule.EventHandlerMap)
        {
            if (eve.Item2.Value->Info.EventId.ContentId != EventHandlerContent.Aetheryte) continue;

            foreach (var obj in eve.Item2.Value->EventObjects)
            {
                if (obj.Value->NameString == LuminaGetter.GetRow<Aetheryte>(0)?.Singular.ExtractText())
                {
                    eventID = eve.Item2.Value->Info.EventId;
                    return true;
                }
            }
        }

        return false;
    }

    private unsafe void OnPostExecuteCommand(ExecuteCommandFlag command, uint param1, uint param2, uint param3, uint param4)
    {
        if (command != ExecuteCommandFlag.Teleport) return;

        var data = DService.AetheryteList.FirstOrDefault(x => x.AetheryteID == param1 && x.SubIndex == param3);
        if (data == null || !UIState.Instance()->IsAetheryteUnlocked(data.AetheryteID)) return;

        EnqueueInstancesCountRetrieve(data.TerritoryID);
    }

    private unsafe void EnqueueInstancesCountRetrieve(uint zoneID)
    {
        if (zoneID == 0 || GameMain.Instance()->CurrentContentFinderConditionId != 0) return;
        
        ServiceConfig.InstancesAmount.TryAdd(CurrentVersion, []);
        SaveConfig(ServiceConfig);

        if (ServiceConfig.InstancesAmount[CurrentVersion].ContainsKey(zoneID)) return;

        TaskHelper.Abort();

        TaskHelper.Enqueue(() => DService.ClientState.TerritoryType == zoneID);
        TaskHelper.Enqueue(() => !BetweenAreas && DService.ObjectTable.LocalPlayer != null && IsScreenReady());
        TaskHelper.Enqueue(() =>
        {
            if (!IsInstancedArea)
                TaskHelper.Abort();
        });

        TaskHelper.Enqueue(() =>
        {
            if (TryGetAetheryteNearby(out var eventID))
                new EventStartPackt(LocalPlayerState.EntityID, eventID).Send();
            else
                TaskHelper.Abort();
        });

        TaskHelper.Enqueue(() =>
        {
            if (!IsAddonAndNodesReady(SelectString)) return false;
            
            var currentInstanceAmount = *InstanceAmountSig.GetStatic<int>();
            if (currentInstanceAmount > 12) return true;

            ServiceConfig.InstancesAmount[CurrentVersion][zoneID] = currentInstanceAmount;
            SaveConfig(ServiceConfig);

            SelectString->Close(true);
            return true;
        });
    }

    internal override void Uninit()
    {
        ExecuteCommandManager.Unregister(OnPostExecuteCommand);
        TaskHelper?.Abort();
    }

    private class Config : OmenServiceConfiguration
    {
        // Version - Zone - Instances Count
        public Dictionary<string, Dictionary<uint, int>> InstancesAmount = [];
    }
}
