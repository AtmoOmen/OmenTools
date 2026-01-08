using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace OmenTools.Extensions;

public static unsafe class LuminaSheetExtensions
{
    #region RowRef<T>

    private static RowRef<T> ToLuminaRowRefInternal<T>(uint id) where T : struct, IExcelRow<T> => 
        new(DService.Instance().Data.Excel, id);

    extension(uint id)
    {
        public RowRef<T> ToLuminaRowRef<T>() where T : struct, IExcelRow<T> => 
            ToLuminaRowRefInternal<T>(id);
    }

    extension(ushort id)
    {
        public RowRef<T> ToLuminaRowRef<T>() where T : struct, IExcelRow<T> => 
            ToLuminaRowRefInternal<T>(id);
    }

    extension(byte id)
    {
        public RowRef<T> ToLuminaRowRef<T>() where T : struct, IExcelRow<T> => 
            ToLuminaRowRefInternal<T>(id);
    }

        #endregion
    
    extension(scoped in Map map)
    {
        public string GetTexturePath() 
        {
            var mapKey = map.Id.ToString();
            var rawKey = mapKey.Replace("/", "");
            return $"ui/map/{mapKey}/{rawKey}_m.tex";
        }

        public List<MapMarker> GetMapMarkers()
        {
            var markerRange = map.MapMarkerRange;
            return LuminaGetter.GetSub<MapMarker>()
                               .SelectMany(x => x)
                               .Where(x => x.RowId == markerRange)
                               .ToList();
        }
    }

    extension(scoped in Aetheryte aetheryte)
    {
        public Vector2 GetPositionWorld()
        {
            var mapRow = aetheryte.Territory.ValueNullable?.Map.ValueNullable;
            if (mapRow == null) return Vector2.Zero;

            return MapToWorld(aetheryte.GetPositionMap(), (Map)mapRow);
        }

        public Vector2 GetPositionMap()
        {
            if (aetheryte.Territory.RowId           == 0 ||
                aetheryte.Territory.Value.Map.RowId == 0)
                return Vector2.Zero;

            var mapRow         = aetheryte.Territory.Value.Map.Value;
            var aetheryteRowID = aetheryte.RowId;
            
            var result = LuminaGetter.GetSub<MapMarker>()
                                     .SelectMany(x => x)
                                     .Where(x => x.DataType == 3 && x.RowId == mapRow.MapMarkerRange && x.DataKey.RowId == aetheryteRowID)
                                     .Select(x => TextureToMap(x.X, x.Y, mapRow.SizeFactor))
                                     .FirstOrDefault();

            return result;
        }
    }

    extension(scoped in MapMarker marker)
    {
        private string GetMarkerPlaceName()
        {
            var placeName = marker.GetMarkerLabel();
            if (placeName != string.Empty) return placeName;

            if (!LuminaGetter.TryGetRow<MapSymbol>(marker.Icon, out var symbol)) return string.Empty;
            return symbol.PlaceName.ValueNullable?.Name.ToString() ?? string.Empty;
        }

        public string GetMarkerLabel() => 
            marker.PlaceNameSubtext.ValueNullable?.Name.ToString() ?? string.Empty;

        public Vector2 GetPosition() => new(marker.X, marker.Y);
    }

    extension(scoped in ClassJob job)
    {
        public BitmapFontIcon ToBitmapFontIcon()
        {
            if (job.RowId == 0) return BitmapFontIcon.NewAdventurer;

            return job.RowId switch
            {
                < 1      => BitmapFontIcon.NewAdventurer,
                < 41     => (BitmapFontIcon)job.RowId + 127,
                41 or 42 => (BitmapFontIcon)job.RowId + 129,
                _        => BitmapFontIcon.NewAdventurer
            };
        }
    }

    extension(scoped in TerritoryType row)
    {
        public string ExtractPlaceName() => 
            row.PlaceName.ValueNullable?.Name.ToString() ?? string.Empty;
    }

    extension(scoped in Level level)
    {
        public Vector3 GetPosition() => 
            new(level.X, level.Y, level.Z);
    }
    
    extension(scoped in ClassJobCategory category)
    {
        public bool IsClassJobIn(uint classJobID) => 
            ClassJobCategory.IsClassJobInCategory(classJobID, category.RowId);
        
        public static bool IsClassJobInCategory(uint classJobID, uint classJobCategoryID)
        {
            if (classJobCategoryID == 0) return false;
        
            var row = Framework.Instance()->ExcelModuleInterface->ExdModule->GetRowBySheetIndexAndRowIndex(60, classJobCategoryID);
            if (row == null) return false;

            return *((byte*)((nint)row->Data + 4) + classJobID) == 1;
        }
    }

    extension(scoped in UIColor color)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => 
            AtkStage.Instance()->AtkUIColorHolder->GetColor(true, color.RowId).ToVector4();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToUInt() => 
            AtkStage.Instance()->AtkUIColorHolder->GetColor(true, color.RowId);
    }
}
