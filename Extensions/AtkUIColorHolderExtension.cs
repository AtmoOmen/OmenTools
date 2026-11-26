using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Extensions;

public static unsafe class AtkUIColorHolderExtension
{
    extension(scoped ref AtkUIColorHolder atkUIColorHolder)
    {
        public static AtkUIColorHolder* Instance() =>
            AtkStage.Instance()->AtkUIColorHolder;

        /// <summary>
        ///     获取当前主题中指定 UIColor 行号的浮点颜色。
        /// </summary>
        /// <param name="colorID">UIColor 行号。</param>
        /// <returns>各分量范围为零到一的 RGBA 颜色。</returns>
        public Vector4 GetColorVector4(uint colorID) =>
            atkUIColorHolder.GetColor(true, colorID).ToVector4();

        /// <summary>
        ///     获取当前主题中指定 UIColor 行号的整数颜色。
        /// </summary>
        /// <param name="colorID">UIColor 行号。</param>
        /// <returns>按游戏原生格式打包的 RGBA 颜色。</returns>
        public uint GetColorUInt(uint colorID) =>
            atkUIColorHolder.GetColor(true, colorID);

        /// <summary>
        ///     获取当前主题中指定 UIColor 行号的字节颜色。
        /// </summary>
        /// <param name="colorID">UIColor 行号。</param>
        /// <returns>各分量以字节表示的 RGBA 颜色。</returns>
        public ByteColor GetColorByte(uint colorID) =>
            new()
            {
                RGBA = atkUIColorHolder.GetColor(true, colorID)
            };
    }
}
