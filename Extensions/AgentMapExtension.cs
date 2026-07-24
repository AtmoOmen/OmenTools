using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService;

namespace OmenTools.Extensions;

public static class AgentMapExtension
{
    extension(scoped ref AgentMap agent)
    {
        /// <summary>
        ///     设置地图后打开地图
        /// </summary>
        public void SetMapByZoneAndOpen
        (
            uint    territoryTypeID,
            string? mapTitle = null
        )
        {
            if (!LuminaGetter.TryGetRow(territoryTypeID, out TerritoryType zoneRow))
                return;

            if (agent.IsAgentActive() && agent.SelectedMapId == zoneRow.Map.RowId)
                agent.Hide();

            if (!string.IsNullOrWhiteSpace(mapTitle))
                agent.MapTitleString.SetString(mapTitle);

            agent.OpenMapByMapId(zoneRow.Map.RowId, territoryTypeID);
        }

        /// <summary>
        ///     设置地图标点后打开地图
        /// </summary>
        public void SetMapAndOpen
        (
            uint    mapID,
            string? mapTitle = null
        )
        {
            if (!LuminaGetter.TryGetRow(mapID, out Map mapRow))
                return;

            if (agent.IsAgentActive() && agent.SelectedMapId == mapID)
                agent.Hide();

            if (!string.IsNullOrWhiteSpace(mapTitle))
                agent.MapTitleString.SetString(mapTitle);

            agent.OpenMapByMapId(mapID, mapRow.TerritoryType.RowId);
        }

        /// <summary>
        ///     设置地图标点后打开地图
        /// </summary>
        public void SetMapFlagByZoneAndOpen
        (
            uint    territoryTypeID,
            Vector3 worldPosition,
            string? mapTitle = null
        )
        {
            if (!LuminaGetter.TryGetRow(territoryTypeID, out TerritoryType zoneRow))
                return;

            if (agent.IsAgentActive() && agent.SelectedMapId == zoneRow.Map.RowId)
                agent.Hide();

            agent.SetFlagMapMarker(territoryTypeID, zoneRow.Map.RowId, worldPosition);
            agent.OpenMap(zoneRow.Map.RowId, territoryTypeID, mapTitle);
        }

        /// <summary>
        ///     设置地图标点后打开地图
        /// </summary>
        public void SetMapFlagAndOpen
        (
            uint    mapID,
            Vector3 worldPosition,
            string? mapTitle = null
        )
        {
            if (!LuminaGetter.TryGetRow(mapID, out Map mapRow))
                return;

            if (agent.IsAgentActive() && agent.SelectedMapId == mapID)
                agent.Hide();

            agent.SetFlagMapMarker(mapRow.TerritoryType.RowId, mapID, worldPosition);
            agent.OpenMap(mapID, mapRow.TerritoryType.RowId, mapTitle);
        }

        /// <summary>
        ///     设置地图标点后打开地图
        /// </summary>
        public void SetMapFlagAndOpen
        (
            Vector3 worldPosition,
            string? mapTitle = null
        ) =>
            agent.SetMapFlagAndOpen(GameState.Map, worldPosition, mapTitle);
    }
}
