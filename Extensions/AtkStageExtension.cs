using FFXIVClientStructs.FFXIV.Client.Enums;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

public static unsafe class AtkStageExtension
{
    extension(scoped ref AtkStage atkStage)
    {
        /// <summary>
        ///     清除输入管理器、碰撞管理器及界面中保存的目标节点焦点和鼠标点击引用。
        /// </summary>
        /// <param name="targetNode">即将释放或需要取消焦点的节点。</param>
        public void ClearNodeFocus(AtkResNode* targetNode)
        {
            if (targetNode is null) return;

            var inputManager = atkStage.AtkInputManager;

            if (inputManager->FocusedNode == targetNode)
                inputManager->FocusedNode = null;

            if ((AtkResNode*)atkStage.AtkCollisionManager->IntersectingCollisionNode == targetNode)
                atkStage.AtkCollisionManager->IntersectingCollisionNode = null;

            for (var index = 0; index < inputManager->SavedMouseClicksCollisionNodes.Length; index++)
            {
                if ((AtkResNode*)inputManager->SavedMouseClicksCollisionNodes[index].Value != targetNode)
                    continue;

                inputManager->SavedMouseClicksCollisionNodes[index] = null;
                inputManager->SavedMouseClicks[index]               = default;
            }

            foreach (ref var focusEntry in inputManager->FocusList)
            {
                if (focusEntry.AtkEventListener is null) continue;

                if (focusEntry.AtkEventTarget == targetNode)
                {
                    focusEntry.AtkEventTarget = null;
                    focusEntry.FocusParam     = 0;

                    var addon = (AtkUnitBase*)focusEntry.AtkEventListener;

                    foreach (ref var node in addon->AdditionalFocusableNodes)
                    {
                        if (node.Value == targetNode)
                            node = null;
                    }
                }
            }
        }

        public void ShowActionTooltip(AtkResNode* node, uint actionID, string? textLabel = null)
        {
            using var stringBuffer = new Utf8String();

            var tooltipType = AtkTooltipType.Action;

            var tooltipArgs = stackalloc AtkTooltipManager.AtkTooltipArgs[1];
            tooltipArgs->Ctor();
            tooltipArgs->ActionArgs.Kind  = DetailKind.Action;
            tooltipArgs->ActionArgs.Id    = (int)actionID;
            tooltipArgs->ActionArgs.Flags = 1;

            if (textLabel is not null)
            {
                tooltipType |= AtkTooltipType.Text;
                stringBuffer.SetString(textLabel);
                tooltipArgs->TextArgs.Text = stringBuffer.StringPtr;
            }

            var addon = RaptureAtkUnitManager.Instance()->GetAddonByNode(node);
            if (addon is null) return;

            atkStage.TooltipManager.ShowTooltip
            (
                tooltipType,
                addon->Id,
                node,
                tooltipArgs
            );
        }

        public void ShowItemTooltip(AtkResNode* node, uint itemID)
        {
            var tooltipArgs = stackalloc AtkTooltipManager.AtkTooltipArgs[1];
            tooltipArgs->Ctor();
            tooltipArgs->ItemArgs.Kind   = DetailKind.ItemId;
            tooltipArgs->ItemArgs.ItemId = (int)itemID;

            var addon = RaptureAtkUnitManager.Instance()->GetAddonByNode(node);
            if (addon is null) return;

            atkStage.TooltipManager.ShowTooltip
            (
                AtkTooltipType.Item,
                addon->Id,
                node,
                tooltipArgs
            );
        }

        public void ShowInventoryItemTooltip(AtkResNode* node, InventoryType container, short slot)
        {
            var tooltipArgs = stackalloc AtkTooltipManager.AtkTooltipArgs[1];
            tooltipArgs->Ctor();
            tooltipArgs->ItemArgs.Kind          = DetailKind.InventoryItem;
            tooltipArgs->ItemArgs.InventoryType = container;
            tooltipArgs->ItemArgs.Slot          = slot;
            tooltipArgs->ItemArgs.BuyQuantity   = -1;
            tooltipArgs->ItemArgs.Flag1         = 0;

            var addon = RaptureAtkUnitManager.Instance()->GetAddonByNode(node);
            if (addon is null) return;

            atkStage.TooltipManager.ShowTooltip
            (
                AtkTooltipType.Item,
                addon->Id,
                node,
                tooltipArgs
            );
        }

        public void HideTooltip(ushort addonID) =>
            atkStage.TooltipManager.HideTooltip(addonID);
    }
}
