using System.Numerics;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Info.Models;
using OmenTools.Interop.Game.Models;

namespace OmenTools.Extensions;

public static unsafe class AtkResNodeExtension
{
    extension
    (
        scoped ref AtkResNode node
    )
    {
        /// <summary>
        ///     获取或设置节点的局部位置，设置时更新节点状态。
        /// </summary>
        public Vector2 Position
        {
            get => new(node.X, node.Y);
            set => node.SetPositionFloat(value.X, value.Y);
        }

        /// <summary>
        ///     获取节点的屏幕位置。
        /// </summary>
        public Vector2 ScreenPosition
            => new(node.ScreenX, node.ScreenY);

        /// <summary>
        ///     获取或设置节点的局部尺寸，设置时更新节点状态。
        /// </summary>
        public Vector2 Size
        {
            get => new(node.GetWidth(), node.GetHeight());
            set
            {
                node.SetWidth((ushort)value.X);
                node.SetHeight((ushort)value.Y);
            }
        }

        /// <summary>
        ///     获取节点在父节点坐标系中的矩形边界。
        /// </summary>
        public Bounds Bounds => new()
        {
            TopLeft     = node.Position,
            BottomRight = node.Position + node.Size
        };

        /// <summary>
        ///     获取节点在父节点坐标系中的中心位置。
        /// </summary>
        public Vector2 Center
            => node.Position + (node.Size / 2.0f);

        /// <summary>
        ///     获取或设置节点的局部缩放，设置时更新节点状态。
        /// </summary>
        public Vector2 Scale
        {
            get => new(node.GetScaleX(), node.GetScaleY());
            set => node.SetScale(value.X, value.Y);
        }

        /// <summary>
        ///     获取或设置节点的旋转角度，单位为度，设置时将角度归入一周范围。
        /// </summary>
        public float RotationDegrees
        {
            get => node.GetRotationDegrees();
            set => node.SetRotationDegrees(value - ((int)(value / 360.0f) * 360.0f));
        }

        /// <summary>
        ///     获取或设置节点的变换原点，设置时更新节点状态。
        /// </summary>
        public Vector2 Origin
        {
            get => new(node.OriginX, node.OriginY);
            set => node.SetOrigin(value.X, value.Y);
        }

        /// <summary>
        ///     获取或设置节点自身的可见状态。
        /// </summary>
        public bool Visible
        {
            get => node.IsVisible();
            set => node.ToggleVisibility(value);
        }

        /// <summary>
        ///     获取或设置节点的 RGBA 颜色，各分量使用归一化数值。
        /// </summary>
        public Vector4 ColorVector
        {
            get => node.Color.ToVector4();
            set => node.Color = value.ToByteColor();
        }

        /// <summary>
        ///     获取或设置节点的 HSVA 颜色。
        /// </summary>
        /// <remarks>
        ///     与 RGBA 颜色相互转换时可能产生舍入误差。
        /// </remarks>
        public ColorHelpers.HsvaColor ColorHsva
        {
            get => ColorHelpers.RgbaToHsv(node.ColorVector);
            set => node.Color = ColorHelpers.HsvToRgb(value).ToByteColor();
        }

        /// <summary>
        ///     获取或设置节点的加算颜色。
        /// </summary>
        /// <remarks>
        ///     各分量使用 0 到 1 之间的归一化数值。
        /// </remarks>
        public Vector3 AddColor
        {
            get => new Vector3(node.AddRed, node.AddGreen, node.AddBlue) / 255.0f;
            set
            {
                node.AddRed   = (short)(value.X * 255);
                node.AddGreen = (short)(value.Y * 255);
                node.AddBlue  = (short)(value.Z * 255);
            }
        }

        /// <summary>
        ///     获取或设置加算颜色的 HSVA 表示。
        /// </summary>
        /// <remarks>
        ///     与 RGBA 颜色相互转换时可能产生舍入误差。
        /// </remarks>
        public ColorHelpers.HsvaColor AddColorHsva
        {
            get => ColorHelpers.RgbaToHsv(node.AddColor.AsVector4());
            set => node.AddColor = ColorHelpers.HsvToRgb(value).AsVector3();
        }

        /// <summary>
        ///     获取或设置节点的乘算颜色。
        /// </summary>
        /// <remarks>
        ///     各分量使用 0 到 1 之间的归一化数值。
        /// </remarks>
        public Vector3 MultiplyColor
        {
            get => new Vector3(node.MultiplyRed, node.MultiplyGreen, node.MultiplyBlue) / 100.0f;
            set
            {
                node.MultiplyRed   = (byte)(value.X * 100.0f);
                node.MultiplyGreen = (byte)(value.Y * 100.0f);
                node.MultiplyBlue  = (byte)(value.Z * 100.0f);
            }
        }

        /// <summary>
        ///     获取或设置乘算颜色的 HSVA 表示。
        /// </summary>
        /// <remarks>
        ///     与 RGBA 颜色相互转换时可能产生舍入误差。
        /// </remarks>
        public ColorHelpers.HsvaColor MultiplyColorHsva
        {
            get => ColorHelpers.RgbaToHsv(node.MultiplyColor.AsVector4());
            set => node.MultiplyColor = ColorHelpers.HsvToRgb(value).AsVector3();
        }

        /// <summary>
        ///     检查节点自身及全部父节点，获取节点是否实际可见。
        /// </summary>
        public bool IsActuallyVisible
        {
            get
            {
                if (!node.Visible)
                    return false;

                var targetNode = node.ParentNode;

                while (targetNode is not null)
                {
                    if (!targetNode->Visible)
                        return false;
                    targetNode = targetNode->ParentNode;
                }

                return true;
            }
        }

        /// <summary>
        ///     将指定标志加入节点标志集合。
        /// </summary>
        public void AddNodeFlag
        (
            params ReadOnlySpan<NodeFlags> flags
        )
        {
            foreach (var flag in flags)
                node.NodeFlags |= flag;
        }

        /// <summary>
        ///     从节点标志集合中移除指定标志。
        /// </summary>
        public void RemoveNodeFlag
        (
            params ReadOnlySpan<NodeFlags> flags
        )
        {
            foreach (var flag in flags)
                node.NodeFlags &= ~flag;
        }

        /// <summary>
        ///     将指定标志加入节点绘制标志集合。
        /// </summary>
        public void AddDrawFlag
        (
            params ReadOnlySpan<DrawFlags> flags
        )
        {
            foreach (var flag in flags)
                node.DrawFlags |= (uint)flag;
        }

        /// <summary>
        ///     从节点绘制标志集合中移除指定标志。
        /// </summary>
        public void RemoveDrawFlag
        (
            params ReadOnlySpan<DrawFlags> flags
        )
        {
            foreach (var flag in flags)
                node.DrawFlags &= ~(uint)flag;
        }

        /// <summary>
        ///     使用整数坐标检查与节点的碰撞，可选择是否包含边界。
        /// </summary>
        public bool CheckCollision
        (
            short x,
            short y,
            bool  inclusive = true
        )
            => node.CheckCollisionAtCoords(x, y, inclusive);

        /// <summary>
        ///     使用向量坐标检查与节点的碰撞，可选择是否包含边界。
        /// </summary>
        public bool CheckCollision
        (
            Vector2 position,
            bool    inclusive = true
        )
            => node.CheckCollisionAtCoords((short)position.X, (short)position.Y, inclusive);

        /// <summary>
        ///     使用原生鼠标事件中的坐标检查与节点的碰撞，可选择是否包含边界。
        /// </summary>
        public bool CheckCollision
        (
            AtkEventData* eventData,
            bool          inclusive = true
        )
            => node.CheckCollisionAtCoords(eventData->MouseData.PosX, eventData->MouseData.PosY, inclusive);

        /// <remarks>
        ///     会自动移除节点本身设置位置的时间线，对于很多界面很有效。
        /// </remarks>
        public void SetFixedPosition
        (
            float x,
            float y
        )
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr->Timeline != null)
                {
                    var resource = nodePtr->Timeline->Resource;

                    for (var animationIndex = 0; animationIndex < resource->AnimationCount; animationIndex++)
                    {
                        ref var positionGroup = ref resource->Animations[animationIndex].KeyGroups[0];
                        for (var keyFrameIndex = 0; keyFrameIndex < positionGroup.KeyFrameCount; keyFrameIndex++)
                            positionGroup.KeyFrames[keyFrameIndex].Value.Float2 = new(x, y);
                    }
                }

                nodePtr->SetPositionFloat(x, y);
            }
        }

        public void OutlineNode()
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return;

                var position = nodePtr->GetPosition();
                var size     = nodePtr->GetSize();

                var nodeVisible = nodePtr->GetVisibility();
                position += ImGui.GetMainViewport().Pos;
                ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport)
                     .AddRect
                     (
                         position,
                         position + size,
                         nodeVisible ?
                             KnownColor.Lime.ToUInt() :
                             KnownColor.CadetBlue.ToUInt()
                     );
            }
        }

        public Vector2 GetPosition()
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return Vector2.Zero;

                var pos = new Vector2(nodePtr->X, nodePtr->Y);
                pos -= new Vector2(nodePtr->OriginX * (nodePtr->ScaleX - 1), nodePtr->OriginY * (nodePtr->ScaleY - 1));
                var par = nodePtr->ParentNode;

                while (par != null)
                {
                    pos *= new Vector2(par->ScaleX,                      par->ScaleY);
                    pos += new Vector2(par->X,                           par->Y);
                    pos -= new Vector2(par->OriginX * (par->ScaleX - 1), par->OriginY * (par->ScaleY - 1));
                    par =  par->ParentNode;
                }

                return pos;
            }
        }

        public Vector2 GetScale()
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return Vector2.One;
                var nodeToCheck = nodePtr;

                var scale = new Vector2(nodeToCheck->ScaleX, nodeToCheck->ScaleY);

                while (nodeToCheck->ParentNode != null)
                {
                    nodeToCheck =  nodeToCheck->ParentNode;
                    scale       *= new Vector2(nodeToCheck->ScaleX, nodeToCheck->ScaleY);
                }

                return scale;
            }
        }

        public Vector2 GetSize()
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return Vector2.One;

                return new Vector2(nodePtr->Width, nodePtr->Height) * nodePtr->GetScale();
            }
        }

        public bool GetVisibility()
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;

                var nodeToCheck = nodePtr;

                while (nodeToCheck != null)
                {
                    if (!nodeToCheck->IsVisible()) return false;
                    nodeToCheck = nodeToCheck->ParentNode;
                }
            }

            return true;
        }

        public NodeState GetNodeState()
        {
            var position = node.GetPosition();
            var size     = node.GetSize();

            return new()
            {
                TopLeft = position,
                Visible = node.GetVisibility(),
                Size    = size
            };
        }

        public bool SetSize(scoped in Vector2 size)
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;

                var isAnyUpdate = false;

                if (NumericRange.UShortRange.Contains((ushort)size.X))
                {
                    nodePtr->Width = (ushort)size.X;
                    isAnyUpdate    = true;
                }

                if (NumericRange.UShortRange.Contains((ushort)size.Y))
                {
                    nodePtr->Height = (ushort)size.Y;
                    isAnyUpdate     = true;
                }

                if (isAnyUpdate)
                    nodePtr->DrawFlags |= 0x1;

                return isAnyUpdate;
            }
        }

        public bool SetSize(scoped in float value)
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;

                var isAnyUpdate = false;

                if (NumericRange.UShortRange.Contains((ushort)value))
                {
                    nodePtr->Width = (ushort)value;
                    isAnyUpdate    = true;
                }

                if (NumericRange.UShortRange.Contains((ushort)value))
                {
                    nodePtr->Height = (ushort)value;
                    isAnyUpdate     = true;
                }

                if (isAnyUpdate)
                    nodePtr->DrawFlags |= 0x1;

                return isAnyUpdate;
            }
        }

        public bool SetSize(scoped in float width, scoped in float height)
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;

                var isAnyUpdate = false;

                if (NumericRange.UShortRange.Contains((ushort)width))
                {
                    nodePtr->Width = (ushort)width;
                    isAnyUpdate    = true;
                }

                if (NumericRange.UShortRange.Contains((ushort)height))
                {
                    nodePtr->Height = (ushort)height;
                    isAnyUpdate     = true;
                }

                if (isAnyUpdate)
                    nodePtr->DrawFlags |= 0x1;

                return isAnyUpdate;
            }
        }

        public bool SetWidth(scoped in float value)
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;

                var isAnyUpdate = false;

                if (NumericRange.UShortRange.Contains((ushort)value))
                {
                    nodePtr->Width = (ushort)value;
                    isAnyUpdate    = true;
                }

                if (isAnyUpdate)
                    nodePtr->DrawFlags |= 0x1;

                return isAnyUpdate;
            }
        }

        public bool SetHeight(scoped in float value)
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return false;

                var isAnyUpdate = false;

                if (NumericRange.UShortRange.Contains((ushort)value))
                {
                    nodePtr->Height = (ushort)value;
                    isAnyUpdate     = true;
                }

                if (isAnyUpdate)
                    nodePtr->DrawFlags |= 0x1;

                return isAnyUpdate;
            }
        }

        public void SetPosition(scoped in Vector2 position)
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return;

                nodePtr->X = position.X;
                nodePtr->Y = position.Y;

                nodePtr->DrawFlags |= 0x1;
            }
        }

        public void SetPosition(scoped in float value)
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return;

                nodePtr->X = value;
                nodePtr->Y = value;

                nodePtr->DrawFlags |= 0x1;
            }
        }

        public void SetPosition(scoped in float x, scoped in float y)
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return;

                nodePtr->X = x;
                nodePtr->Y = y;

                nodePtr->DrawFlags |= 0x1;
            }
        }

        public void SetPositionX(scoped in float value)
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return;

                nodePtr->X = value;

                nodePtr->DrawFlags |= 0x1;
            }
        }

        public void SetPositionY(scoped in float value)
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                if (nodePtr == null) return;

                nodePtr->Y = value;

                nodePtr->DrawFlags |= 0x1;
            }
        }

        public AtkUnitBase* GetOwnerAddon()
        {
            fixed (AtkResNode* nodePtr = &node)
            {
                return nodePtr == null ?
                           null :
                           RaptureAtkUnitManager.Instance()->GetAddonByNode(nodePtr);
            }
        }
    }
}
