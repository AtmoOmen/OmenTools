using System.Numerics;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Helpers;

public static unsafe class AtkResNodeExtensions
{
    extension(scoped ref AtkResNode node)
    {
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
                     .AddRect(position, 
                              position + size, 
                              nodeVisible ? KnownColor.Lime.ToUInt() : KnownColor.CadetBlue.ToUInt());
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
                Size    = size,
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
                    isAnyUpdate = true;
                }

                if (NumericRange.UShortRange.Contains((ushort)size.Y))
                {
                    nodePtr->Height = (ushort)size.Y;
                    isAnyUpdate = true;
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
                if (nodePtr == null) return null;
                
                return RaptureAtkUnitManager.Instance()->GetAddonByNode(nodePtr);
            }
        }
    }
}
