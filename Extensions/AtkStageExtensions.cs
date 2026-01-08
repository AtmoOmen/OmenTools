using FFXIVClientStructs.FFXIV.Client.Enums;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

public static unsafe class AtkStageExtensions
{
    extension(scoped ref AtkStage atkStage)
    {
        public void ClearNodeFocus(AtkResNode* targetNode)
        {
            if (targetNode is null) return;

            foreach (ref var focusEntry in atkStage.AtkInputManager->FocusList)
            {
                if (focusEntry.AtkEventListener is null) continue;
                
                if (focusEntry.AtkEventTarget == targetNode)
                {
                    focusEntry.AtkEventTarget = null;
                    focusEntry.FocusParam     = 0;
                    
                    atkStage.AtkInputManager->FocusedNode                   = null;
                    atkStage.AtkCollisionManager->IntersectingCollisionNode = null;
                    
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

            var tooltipType = AtkTooltipManager.AtkTooltipType.Action;

            var tooltipArgs = stackalloc AtkTooltipManager.AtkTooltipArgs[1];
            tooltipArgs->Ctor();
            tooltipArgs->ActionArgs.Kind  = DetailKind.Action;
            tooltipArgs->ActionArgs.Id    = (int)actionID;
            tooltipArgs->ActionArgs.Flags = 1;

            if (textLabel is not null)
            {
                tooltipType |= AtkTooltipManager.AtkTooltipType.Text;
                stringBuffer.SetString(textLabel);
                tooltipArgs->TextArgs.Text = stringBuffer.StringPtr;
            }

            var addon = RaptureAtkUnitManager.Instance()->GetAddonByNode(node);
            if (addon is null) return;

            atkStage.TooltipManager.ShowTooltip(
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

            atkStage.TooltipManager.ShowTooltip(
                AtkTooltipManager.AtkTooltipType.Item,
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

            atkStage.TooltipManager.ShowTooltip(
                AtkTooltipManager.AtkTooltipType.Item,
                addon->Id,
                node,
                tooltipArgs
            );
        }

        public void HideTooltip(ushort addonID) => 
            atkStage.TooltipManager.HideTooltip(addonID);
    }
}
