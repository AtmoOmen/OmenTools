using System.Numerics;
using Dalamud.Game.NativeWrapper;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Info.Models;
using OmenTools.Interop.Game.Models;

namespace OmenTools.Extensions;

public static unsafe class AtkUnitBaseExtension
{
    extension(scoped ref AtkUnitBase addon)
    {
        /// <summary>
        ///     获取界面位置。
        /// </summary>
        public Vector2 Position => new(addon.X, addon.Y);

        /// <summary>
        ///     获取或设置界面大小，同时更新窗口节点及碰撞节点列表。
        /// </summary>
        public Vector2 Size
        {
            get
            {
                ushort width  = 0;
                ushort height = 0;
                addon.GetSize(&width, &height, false);
                return new Vector2(width, height);
            }
            set => addon.Resize(value);
        }

        /// <summary>
        ///     获取根节点大小；根节点不存在时返回零。
        /// </summary>
        public Vector2 RootSize => addon.RootNode is null ?
                                       Vector2.Zero :
                                       new Vector2(addon.RootNode->Width, addon.RootNode->Height);

        /// <summary>
        ///     获取界面及根节点是否均处于可见状态。
        /// </summary>
        public bool IsActuallyVisible =>
            addon.IsVisible             &&
            addon.RootNode is not null  &&
            addon.RootNode->IsVisible() &&
            (addon.VisibilityFlags & 5) is 0;

        /// <summary>
        ///     更新界面、窗口与标题栏碰撞节点的大小，并重建绘制和碰撞节点列表。
        /// </summary>
        /// <param name="newSize">新的界面宽度与高度。</param>
        public void Resize(Vector2 newSize)
        {
            var windowNode = addon.WindowNode;
            if (windowNode is null)
                return;

            windowNode->SetWidth((ushort)newSize.X);
            windowNode->SetHeight((ushort)newSize.Y);

            if (addon.WindowHeaderCollisionNode is not null)
                addon.WindowHeaderCollisionNode->SetWidth((ushort)(newSize.X - 14.0f));

            addon.SetSize((ushort)newSize.X, (ushort)newSize.Y);
            windowNode->Component->UldManager.UpdateDrawNodeList();
            addon.UpdateCollisionNodeList(false);
        }

        public T* As<T>() where T : unmanaged
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return null;

                return (T*)addonPtr;
            }
        }

        public void CallbackNoUpdate(params object[] args)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return;

                using var atkValues = new AtkValueArray(args);
                addonPtr->FireCallback((uint)atkValues.Length, atkValues.Pointer);
            }
        }

        /// <remarks>请自行处理处理传入的 AtkValueArray 生命周期</remarks>
        public void CallbackNoUpdate(scoped in AtkValueArray atkValues)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return;
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

        /// <remarks>请自行处理处理传入的 AtkValueArray 生命周期</remarks>
        public void Callback(scoped in AtkValueArray atkValues)
        {
            fixed (AtkUnitBase* addonPtr = &addon)
            {
                if (addonPtr == null) return;

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
                    result      = true;
                }

                if (NumericRange.ShortRange.Contains((short)position.Y))
                {
                    addonPtr->Y = (short)position.Y;
                    result      = true;
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
                    result      = true;
                }

                if (NumericRange.ShortRange.Contains((short)y))
                {
                    addonPtr->Y = (short)y;
                    result      = true;
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

                var windowNode = windowNodeID != null ?
                                     (AtkComponentNode*)addonPtr->GetNodeById(windowNodeID.Value) :
                                     addonPtr->WindowNode;
                if (windowNode == null) return string.Empty;

                var mainTitle     = string.Empty;
                var mainTitleNode = (AtkTextNode*)windowNode->Component->UldManager.SearchNodeById(nodeIDs.Value.MainTitleNodeID);
                if (mainTitleNode != null)
                    mainTitle = mainTitleNode->GetText().ToString();

                var subTitle     = string.Empty;
                var subTitleNode = (AtkTextNode*)windowNode->Component->UldManager.SearchNodeById(nodeIDs.Value.SubTitleNodeID);
                if (subTitleNode != null)
                    subTitle = subTitleNode->GetText().ToString();

                var windowTitle = !string.IsNullOrWhiteSpace(subTitle) ?
                                      subTitle :
                                      mainTitle;
                return windowTitle;
            }
        }

        public void ClickComponent
        (
            AtkComponentNode*    target,
            int                  which,
            AtkEventType         type,
            ManagedAtkEvent?     eventData = null,
            ManagedAtkInputData? inputData = null
        )
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

        public T* ToStruct<T>() where T : unmanaged =>
            (T*)addon.Address;
    }

    extension(nint ptr)
    {
        public AtkUnitBase* ToAtkUnitBase() =>
            (AtkUnitBase*)ptr;
    }
}
