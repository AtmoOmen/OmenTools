using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class InstancesManager : OmenServiceBase<InstancesManager>
{
    public static unsafe bool   IsInstancedArea => UIState.Instance()->PublicInstance.IsInstancedArea();
    public static unsafe uint   CurrentInstance => UIState.Instance()->PublicInstance.InstanceId;
    public static unsafe string CurrentVersion  => Framework.Instance()->GameVersionString;
    public static unsafe int    InstanceAmount  => IsInstancedArea && InstanceAmountPtr != null ? *InstanceAmountPtr : 0;

    public int GetInstancesCount(uint zoneID = 0)
    {
        if (zoneID == 0)
            zoneID = GameState.TerritoryType;

        if (ServiceConfig.InstancesAmount.TryAdd(CurrentVersion, []))
            ServiceConfig.Save(Instance());

        return ServiceConfig.InstancesAmount[CurrentVersion].GetValueOrDefault(zoneID, 0);
    }


    private static readonly CompSig InstanceAmountSig = new("4C 8D 0D ?? ?? ?? ?? 44 0F B7 41");
    private static unsafe   int*    InstanceAmountPtr = InstanceAmountSig.GetStatic<int>();

    private TaskHelper? TaskHelper;
    private Config      ServiceConfig = null!;

    internal override void Init()
    {
        ServiceConfig =   LoadConfig<Config>() ?? new();
        TaskHelper    ??= new() { TimeoutMS = 15_000 };

        ExecuteCommandManager.Instance().RegPost(OnPostExecuteCommand);
        GameState.Instance().Login += OnLogin;

        if (DService.Instance().ClientState.IsLoggedIn)
            EnqueueInstancesCountRetrieve(GameState.TerritoryType);
    }

    internal override void Uninit()
    {
        ExecuteCommandManager.Instance().Unreg(OnPostExecuteCommand);
        GameState.Instance().Login -= OnLogin;

        TaskHelper?.Abort();
        TaskHelper?.Dispose();
        TaskHelper = null;
    }

    private unsafe void OnPostExecuteCommand(ExecuteCommandFlag command, uint param1, uint param2, uint param3, uint param4)
    {
        if (command != ExecuteCommandFlag.Teleport) return;

        if (!LuminaGetter.TryGetRow(param1, out Aetheryte data) || !UIState.Instance()->IsAetheryteUnlocked(data.RowId)) return;

        EnqueueInstancesCountRetrieve(data.Territory.RowId);
    }

    private void OnLogin() =>
        EnqueueInstancesCountRetrieve(GameState.TerritoryType);

    private unsafe void EnqueueInstancesCountRetrieve(uint zoneID)
    {
        if (zoneID == 0 || GameMain.Instance()->CurrentContentFinderConditionId != 0) return;

        ServiceConfig.InstancesAmount.TryAdd(CurrentVersion, []);
        SaveConfig(ServiceConfig);

        if (ServiceConfig.InstancesAmount[CurrentVersion].ContainsKey(zoneID)) return;

        TaskHelper.Abort();

        TaskHelper.Enqueue(() => GameState.TerritoryType == zoneID);
        TaskHelper.Enqueue(() => !BetweenAreas && DService.Instance().ObjectTable.LocalPlayer != null && UIModule.IsScreenReady());
        TaskHelper.Enqueue
        (() =>
            {
                if (!IsInstancedArea)
                    TaskHelper.Abort();
            }
        );

        TaskHelper.Enqueue
        (() =>
            {
                if (EventFramework.Instance()->TryGetAetheryteNearby(out var eventID))
                    new EventStartPackt(LocalPlayerState.EntityID, eventID).Send();
                else
                    TaskHelper.Abort();
            }
        );

        TaskHelper.Enqueue
        (() =>
            {
                if (!SelectString->IsAddonAndNodesReady()) return false;

                var currentInstanceAmount = *InstanceAmountSig.GetStatic<int>();
                if (currentInstanceAmount > 12) return true;

                ServiceConfig.InstancesAmount[CurrentVersion][zoneID] = currentInstanceAmount;
                SaveConfig(ServiceConfig);

                SelectString->Close(true);
                return true;
            }
        );
    }

    private class Config : OmenServiceConfiguration
    {
        // Version - Zone - Instances Count
        public Dictionary<string, Dictionary<uint, int>> InstancesAmount = [];
    }
}
