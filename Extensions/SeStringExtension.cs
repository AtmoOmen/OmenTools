using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using OmenTools.Interop.Game.Helpers;
using OmenTools.Interop.Game.Lumina;
using DSeString = Dalamud.Game.Text.SeStringHandling.SeString;
using DSeStringBuilder = Dalamud.Game.Text.SeStringHandling.SeStringBuilder;
using LSeStringBuilder = Lumina.Text.SeStringBuilder;

namespace OmenTools.Extensions;

public static class SeStringExtension
{
    private const char SE_SQUARE_COUNT_BASE_CHAR = '\uE08F';
    private const char SE_SMALL_COUNT_BASE_CHAR  = '\uE060';
    private const char SE_HEX_COUNT_BASE_CHAR    = '\uE0B0';

    private static readonly Lazy<(int Start, int End, ulong[] Bitmap)> SEIconBitmap =
        new
        (() =>
            {
                var seIcons    = Enum.GetValues<SeIconChar>().Select(i => (int)i).ToList();
                var start      = seIcons.Min();
                var end        = seIcons.Max();
                var range      = end - start + 1;
                var bitmapSize = (range + 63) / 64; // 向上取整到最接近的 64 的倍数
                var bitmap     = new ulong[bitmapSize];

                foreach (var icon in seIcons)
                {
                    var adjustedValue = icon - start;
                    var index         = adjustedValue >> 6;
                    var bit           = adjustedValue & 63;
                    bitmap[index] |= 1UL << bit;
                }

                return (start, end, bitmap);
            }
        );

    extension<T>
    (
        T value
    ) where T : INumber<T>
    {
        public string ToSEHexCount
        (
            string? format = null
        )
        {
            var raw = value.ToString(format, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(raw))
                return string.Empty;
            return string.Create
            (
                raw.Length,
                raw,
                (span, state) =>
                {
                    for (var i = 0; i < state.Length; i++)
                    {
                        var c = state[i];

                        if (char.IsAsciiDigit(c))
                            span[i] = (char)(SE_HEX_COUNT_BASE_CHAR + (c - '0'));
                        else
                            span[i] = c;
                    }
                }
            );
        }

        public string ToSESmallCount
        (
            string? format = null
        )
        {
            var raw = value.ToString(format, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(raw))
                return string.Empty;
            return string.Create
            (
                raw.Length,
                raw,
                (span, state) =>
                {
                    for (var i = 0; i < state.Length; i++)
                    {
                        var c = state[i];

                        if (char.IsAsciiDigit(c))
                            span[i] = (char)(SE_SMALL_COUNT_BASE_CHAR + (c - '0'));
                        else
                            span[i] = c;
                    }
                }
            );
        }

        public string ToSESquareCount
        (
            string? format = null
        )
        {
            var raw = value.ToString(format, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(raw))
                return string.Empty;
            return string.Create
            (
                raw.Length,
                raw,
                (span, state) =>
                {
                    for (var i = 0; i < state.Length; i++)
                    {
                        var c = state[i];

                        if (char.IsAsciiDigit(c))
                            span[i] = (char)(SE_SQUARE_COUNT_BASE_CHAR + (c - '0'));
                        else
                            span[i] = c;
                    }
                }
            );
        }
    }

    extension
    (
        LSeStringBuilder b
    )
    {
        public LSeStringBuilder AppendDalamudSeString
        (
            DSeString value
        ) =>
            b.Append(new ReadOnlySeString(value.Encode()));

        public LSeStringBuilder AppendDalamudSeString
        (
            DSeStringBuilder value
        ) =>
            b.Append(value.Build().Encode());

        public LSeStringBuilder AppendDalamudSeString
        (
            Payload value
        ) =>
            b.Append(new ReadOnlySeString(new DSeString(value).Encode()));

        public LSeStringBuilder AppendRentedSeStringBuilder
        (
            RentedSeStringBuilder value
        ) =>
            b.Append(value.Builder.ToReadOnlySeString());

        public LSeStringBuilder AppendIcon
        (
            BitmapFontIcon icon
        ) =>
            b.AppendIcon((uint)icon);

        public LSeStringBuilder AppendIcon
        (
            SeIconChar icon
        ) =>
            b.Append(icon.ToIconString());

        public LSeStringBuilder AppendFormattable
        (
            IFormattable    value,
            IFormatProvider provider
        ) =>
            b.Append(value.ToString(null, provider) ?? string.Empty);
    }

    // TODO: API 16 移除
    extension
    (
        DSeStringBuilder b
    )
    {
        public DSeStringBuilder AddRange
        (
            IEnumerable<Payload> payloads
        )
        {
            foreach (var x in payloads)
                b = b.Add(x);

            return b;
        }
    }

    extension
    (
        ReadOnlySeString
    )
    {
        public static ReadOnlySeString FormatNotFoundCommandParamLogMessage
        (
            int              index,
            ReadOnlySeString paramName,
            ReadOnlySeString paramValue
        ) =>
            ISeStringEvaluator.Instance().EvaluateFromLogMessage
            (
                3802,
                [
                    index,
                    paramName,
                    paramValue
                ]
            );
        
        public static ReadOnlySeString FormatIncorrectCommandParamLogMessage
        (
            int              index,
            ReadOnlySeString paramName,
            ReadOnlySeString paramValue
        ) =>
            ISeStringEvaluator.Instance().EvaluateFromLogMessage
            (
                3803,
                [
                    index,
                    paramName,
                    paramValue
                ]
            );

        public static ReadOnlySeString FormatLink
        (
            DalamudLinkPayload link,
            ReadOnlySeString   text
        )
        {
            using var rented  = new RentedSeStringBuilder();
            var       builder = rented.Builder;

            var message = ISeStringEvaluator.Instance().EvaluateFromAddon(371, [text]);

            builder
                .AppendDalamudSeString(link)
                .Append(message)
                .PopLink();

            return builder.ToReadOnlySeString();
        }
        
        public static ReadOnlySeString CreatePlayerLink
        (
            string            playerName,
            uint              worldID             = 0,
            ReadOnlySeString? displayNameOverride = null
        )
        {
            ArgumentNullException.ThrowIfNull(playerName);

            using var builder = new RentedSeStringBuilder();

            if (displayNameOverride is { } displayName)
            {
                // ignored
            }
            else
                displayName = ReadOnlySeString.CreatePlayer(playerName, worldID);

            builder.PushLinkCharacter(playerName, worldID)
                   .Append(displayName)
                   .PopLink();

            return builder.ToReadOnlySeString();
        }

        /// <summary>
        ///     非链接，仅为游戏原生风格的富文本
        /// </summary>
        public static ReadOnlySeString CreatePlayer
        (
            string playerName,
            uint   worldID = 0
        )
        {
            ArgumentNullException.ThrowIfNull(playerName);

            using var builder = new RentedSeStringBuilder();

            builder.Append(playerName);

            if (worldID != 0 && GameState.HomeWorld != worldID)
            {
                if (!LuminaGetter.TryGetRow<World>(worldID, out var worldRow))
                    throw new InvalidDataException("无效的 World。");

                builder.AppendIcon((uint)BitmapFontIcon.CrossWorld)
                       .Append(worldRow.Name);
            }

            return builder.ToReadOnlySeString();
        }

        public static ReadOnlySeString CreateAchievementLink
        (
            uint              achievementID,
            ReadOnlySeString? displayNameOverride = null
        )
        {
            if (!LuminaGetter.TryGetRow<Achievement>(achievementID, out var achievementRow))
                throw new InvalidDataException("无效的 Achievement。");

            using var builder = new RentedSeStringBuilder();

            if (displayNameOverride is { } displayName)
            {
                // ignored
            }
            else
                displayName = achievementRow.Name;

            displayName = ISeStringEvaluator.Instance().EvaluateFromAddon(371, [displayName]);

            builder.PushLinkAchievement(achievementID)
                   .Append(displayName)
                   .PopLink();

            return builder.ToReadOnlySeString();
        }

        public static ReadOnlySeString CreateMapLink
        (
            Vector3           worldPosition,
            uint              territoryType       = 0,
            uint              map                 = 0,
            ReadOnlySeString? displayNameOverride = null
        )
        {
            if (territoryType == 0)
                territoryType = GameState.TerritoryType;

            if (!LuminaGetter.TryGetRow<TerritoryType>(territoryType, out var territoryTypeRow))
                throw new InvalidDataException("无效的 TerritoryType。");

            if (map == 0)
            {
                map = territoryType == GameState.TerritoryType ?
                          GameState.Map :
                          territoryTypeRow.Map.RowId;
            }

            if (!LuminaGetter.TryGetRow<Map>(map, out var mapRow))
                throw new InvalidDataException("无效的 Map。");
            if (mapRow.TerritoryType.RowId != territoryType)
                throw new InvalidDataException("Map 对应的 TerritoryType 与传入的 TerritoryType 不一致。");

            using var builder = new RentedSeStringBuilder();

            var mapPosition = PositionHelper.WorldToMap(worldPosition, mapRow);

            if (displayNameOverride is { } displayName)
            {
                // ignored
            }
            else
            {
                displayName = territoryTypeRow.ExtractPlaceName();

                if (territoryType == GameState.TerritoryType)
                {
                    if (InstancesManager.IsInstancedArea)
                    {
                        builder.Append(displayName)
                               .Append(InstancesManager.CurrentInstance.ToSEHexCount())
                               .Append($" ( {mapPosition.X:F1}  , {mapPosition.Y:F1} )");
                    }
                    else
                    {
                        builder.Append(displayName)
                               .Append($" ( {mapPosition.X:F1}  , {mapPosition.Y:F1} )");
                    }

                    displayName = builder.ToReadOnlySeString();
                    builder.Clear();
                }
            }

            displayName = ISeStringEvaluator.Instance().EvaluateFromAddon(371, [displayName]);

            builder.PushLinkMapPosition(territoryType, map, (int)(worldPosition.X * 1000), (int)(worldPosition.Z * 1000))
                   .Append(displayName)
                   .PopLink();

            return builder.ToReadOnlySeString();
        }

        public static ReadOnlySeString CreateItemLink
        (
            uint              itemID,
            bool              isHQ                = false,
            ReadOnlySeString? displayNameOverride = null
        ) =>
            ReadOnlySeString.CreateItemLink
            (
                itemID,
                isHQ ?
                    ItemKind.Hq :
                    ItemKind.Normal,
                displayNameOverride
            );

        public static ReadOnlySeString CreateItemLink
        (
            uint              itemID,
            ItemKind          kind                = ItemKind.Normal,
            ReadOnlySeString? displayNameOverride = null
        )
        {
            var rawID = ItemUtil.GetRawId(itemID, kind);

            var displayName = displayNameOverride ?? ItemUtil.GetItemName(rawID);
            if (displayName.IsEmpty)
                throw new Exception("无法确定物品名称。");

            var textColor     = ItemUtil.GetItemRarityColorType(rawID);
            var textEdgeColor = textColor + 1u;

            using var rssb    = new RentedSeStringBuilder();
            var       builder = rssb.Builder;

            var itemName = rssb.Builder
                               .PushColorType(textColor)
                               .PushEdgeColorType(textEdgeColor)
                               .Append(displayName)
                               .PopEdgeColorType()
                               .PopColorType()
                               .ToReadOnlySeString();

            itemName = ISeStringEvaluator.Instance().EvaluateFromAddon(371, [itemName]);

            builder.Clear();

            builder.PushLinkItem(itemID)
                   .Append(itemName)
                   .PopLink();

            return builder.ToReadOnlySeString();
        }

        /// <summary>
        ///     非链接，仅为游戏原生风格的富文本
        /// </summary>
        public static ReadOnlySeString CreateItem
        (
            uint              itemID,
            bool              isHQ                = false,
            ReadOnlySeString? displayNameOverride = null
        ) =>
            ReadOnlySeString.CreateItem
            (
                itemID,
                isHQ ?
                    ItemKind.Hq :
                    ItemKind.Normal,
                displayNameOverride
            );

        /// <summary>
        ///     非链接，仅为游戏原生风格的富文本
        /// </summary>
        public static ReadOnlySeString CreateItem
        (
            uint              itemID,
            ItemKind          kind                = ItemKind.Normal,
            ReadOnlySeString? displayNameOverride = null
        )
        {
            var rawID = ItemUtil.GetRawId(itemID, kind);

            var displayName = displayNameOverride ?? ItemUtil.GetItemName(rawID);
            if (displayName.IsEmpty)
                throw new Exception("无法确定物品名称。");

            var textColor     = ItemUtil.GetItemRarityColorType(rawID);
            var textEdgeColor = textColor + 1u;

            using var rssb = new RentedSeStringBuilder();

            var itemName = rssb.Builder
                               .PushColorType(textColor)
                               .PushEdgeColorType(textEdgeColor)
                               .Append(displayName)
                               .PopEdgeColorType()
                               .PopColorType()
                               .ToReadOnlySeString();

            return itemName;
        }
    }

    extension
    (
        string input
    )
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SanitizeSEIcon() => input.AsSpan().SanitizeSEIcon();
    }

    extension
    (
        ReadOnlySpan<char> input
    )
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SanitizeSEIcon()
        {
            if (input.IsEmpty) return string.Empty;

            var (start, end, bitmap) = SEIconBitmap.Value;
            Span<char> output      = stackalloc char[input.Length];
            var        outputIndex = 0;

            foreach (var c in input)
            {
                if (c < start || c > end || !c.IsSEIcon(start, bitmap))
                    output[outputIndex++] = c;
            }

            return new string(output[..outputIndex]);
        }
    }

    extension
    (
        char c
    )
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSEIcon
        (
            int     start,
            ulong[] bitmap
        )
        {
            var adjustedValue = c - start;
            var index         = adjustedValue >> 6;
            var bit           = adjustedValue              & 63;
            return index < bitmap.Length && (bitmap[index] & (1UL << bit)) != 0;
        }
    }
}
