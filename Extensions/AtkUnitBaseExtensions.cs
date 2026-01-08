using System.Numerics;
using Dalamud.Game.NativeWrapper;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

public static unsafe class AtkUnitBaseExtensions
{
    extension(scoped ref AtkUnitBase addon)
    {
        public void CallbackNoUpdate(params object[] args)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return;

                using var atkValues = new AtkValueArray(args);
                addonPtr->FireCallback((uint)atkValues.Length, atkValues.Pointer);
            }
        }
        
        public void Callback(params object[] args)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return;

                using var atkValues = new AtkValueArray(args);
                addonPtr->FireCallback((uint)atkValues.Length, atkValues.Pointer, true);
            }
        }
        
        public bool IsAddonAndNodesReady()
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                return addonPtr != null    &&
                       addonPtr->IsVisible &&
                       addonPtr->IsFullyLoaded();
            }
        }
        
        public bool SetPosition(scoped in Vector2 position)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return false;

                var result = false;
                if (NumericRange.ShortRange.Contains((short)position.X))
                {
                    addonPtr->X = (short)position.X;
                    result = true;
                }
                
                if (NumericRange.ShortRange.Contains((short)position.Y))
                {
                    addonPtr->Y = (short)position.Y;
                    result     = true;
                }

                return result;
            }
        }
        
        public bool SetPosition(scoped in float value)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return false;

                if (NumericRange.ShortRange.Contains((short)value))
                {
                    addonPtr->X = (short)value;
                    addonPtr->Y = (short)value;
                    return true;
                }

                return false;
            }
        }
        
        public bool SetPosition(scoped in float x, scoped in float y)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return false;
                
                var result = false;
                if (NumericRange.ShortRange.Contains((short)x))
                {
                    addonPtr->X = (short)x;
                    result     = true;
                }

                if (NumericRange.ShortRange.Contains((short)y))
                {
                    addonPtr->Y = (short)y;
                    result     = true;
                }

                return result;
            }
        }

        public bool SetPositionX(scoped in float value)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return false;

                if (NumericRange.ShortRange.Contains((short)value))
                {
                    addonPtr->X = (short)value;
                    return true;
                }

                return false;
            }
        }

        public bool SetPositionY(scoped in float value)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return false;

                if (NumericRange.ShortRange.Contains((short)value))
                {
                    addonPtr->Y = (short)value;
                    return true;
                }

                return false;
            }
        }
        
        // 这是一个古老的手工匠心获取流程, 非常不稳定, 但为了向后兼容所以
        public string GetWindowTitle(uint? windowNodeID = null, (uint MainTitleNodeID, uint SubTitleNodeID)? nodeIDs = null)
        {
            nodeIDs ??= (3, 4);

            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (!addonPtr->IsAddonAndNodesReady()) return string.Empty;
        
                var windowNode = windowNodeID != null ? (AtkComponentNode*)addonPtr->GetNodeById(windowNodeID.Value) : addonPtr->WindowNode;
                if (windowNode == null) return string.Empty;
                
                var mainTitle     = string.Empty;
                var mainTitleNode = (AtkTextNode*)windowNode->Component->UldManager.SearchNodeById(nodeIDs.Value.MainTitleNodeID);
                if (mainTitleNode != null)
                    mainTitle = mainTitleNode->GetText().ToString();
                
                var subTitle     = string.Empty;
                var subTitleNode = (AtkTextNode*)windowNode->Component->UldManager.SearchNodeById(nodeIDs.Value.SubTitleNodeID);
                if (subTitleNode != null)
                    subTitle = subTitleNode->GetText().ToString();
                
                var windowTitle = !string.IsNullOrWhiteSpace(subTitle) ? subTitle : mainTitle;
                return windowTitle;
            }
        }

        public void ClickComponent(
            AtkComponentNode*    target,
            int                  which,
            AtkEventType         type,
            ManagedAtkEvent?     eventData = null,
            ManagedAtkInputData? inputData = null)
        {
            if (target == null) return;
            
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return;

                eventData ??= ManagedAtkEvent.ForNormalTarget(target, addonPtr);
                inputData ??= ManagedAtkInputData.Empty();

                addonPtr->AtkEventListener.ReceiveEvent(type, which, eventData.AtkEvent, inputData.AtkEventData);
            }
        }
        
        public void ClickAtkStage(int which, AtkEventType type = AtkEventType.MouseClick)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return;

                var target = AtkStage.Instance();

                var eventData = ManagedAtkEvent.ForNormalTarget(target, addonPtr);
                var inputData = ManagedAtkInputData.Empty();

                addonPtr->AtkEventListener.ReceiveEvent(type, which, eventData.AtkEvent, inputData.AtkEventData);
            }
        }
    }
    
    extension(scoped in AtkUnitBasePtr addon)
    {
        public AtkUnitBase* ToStruct() =>
            (AtkUnitBase*)addon.Address;
    }

    extension(nint ptr)
    {
        public AtkUnitBase* ToAtkUnitBase() =>
            (AtkUnitBase*)ptr;
    }
}
