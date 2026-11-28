using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

// 单独开一个文件方法太少了，就会扔到这里
public static unsafe class AtkExtension
{
    extension(scoped ref AtkImageNode node)
    {
        /// <summary>
        ///     获取图像节点当前选中纹理部件的图标行号。
        /// </summary>
        /// <returns>图标行号；纹理部件或资源无效时返回零。</returns>
        public uint GetIconID()
        {
            fixed (AtkImageNode* ptr = &node)
            {
                if (ptr == null)
                    return 0;

                var partList = ptr->PartsList;
                if (partList == null)
                    return 0;
                if (ptr->PartId >= partList->PartCount)
                    return 0;

                var parts = partList->Parts;
                if (parts == null)
                    return 0;

                var asset = parts[ptr->PartId].UldAsset;
                if (asset == null)
                    return 0;
                if (asset->AtkTexture.TextureType != TextureType.Resource)
                    return 0;

                var resource = asset->AtkTexture.Resource;
                if (resource == null)
                    return 0;

                return resource->IconId;
            }
        }
    }

    extension(scoped ref AtkTextNode node)
    {
        /// <summary>
        ///     测量文本节点当前文本在当前字体、字号与排版设置下的绘制尺寸。
        /// </summary>
        /// <param name="considerScale">
        ///     是否计入节点渲染变换矩阵（<see cref="AtkResNode.Transform" />）的累积缩放（包含节点自身缩放、父级与窗口缩放及全局界面缩放），默认为 <see langword="false" />。
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 <see langword="false" />：返回节点本地逻辑坐标系下的未缩放尺寸，适用于绝大多数原生界面内部布局计算（如设置节点逻辑宽高、排布同级节点相对坐标、计算窗口内容尺寸，或与节点逻辑尺寸比较以裁剪或缩减字号）。
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 <see langword="true" />：返回乘以累积缩放倍率后的屏幕实际绘制像素尺寸，适用于需要与屏幕像素坐标对齐的外部绘制或边界计算（如 ImGui 覆盖层叠加绘制、屏幕空间命中检测等；节点尚未挂入已渲染界面树或变换矩阵未刷新时可能未反映最终缩放）。
        ///             </description>
        ///         </item>
        ///     </list>
        /// </param>
        /// <returns>当前文本的绘制宽高。</returns>
        public Vector2 GetTextDrawSize(bool considerScale = false)
        {
            fixed (AtkTextNode* ptr = &node)
            {
                ushort sizeX = 0;
                ushort sizeY = 0;

                ptr->GetTextDrawSize(&sizeX, &sizeY, ptr->NodeText.StringPtr, considerScale: considerScale);
                return new Vector2(sizeX, sizeY);
            }
        }
    }

    extension(scoped ref AtkCollisionNode target)
    {
        public void Click(AtkEvent* eventData)
        {
            fixed (AtkCollisionNode* ptr = &target)
            {
                if (ptr == null) return;
                ptr->AtkResNode.GetOwnerAddon()->ReceiveEvent(eventData->State.EventType, (int)eventData->Param, eventData);
            }
        }

        public void Click()
        {
            fixed (AtkCollisionNode* ptr = &target)
            {
                if (ptr == null) return;

                var btnRes = ptr->AtkResNode;
                var evt    = btnRes.AtkEventManager.Event;

                while (evt->State.EventType != AtkEventType.MouseClick)
                    evt = evt->NextEvent;

                btnRes.GetOwnerAddon()->ReceiveEvent(evt->State.EventType, (int)evt->Param, evt);
            }
        }
    }
}
