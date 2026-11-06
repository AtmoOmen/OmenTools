using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Lumina;
using AgentMapExpansion = FFXIVClientStructs.Expansion.FFXIV.Client.UI.Agent.AgentMap;

namespace OmenTools.Extensions;

public static class AgentMapExtension
{
    extension
    (
        scoped ref AgentMapExpansion agent
    )
    {
        public void SetMapByZoneAndOpen
        (
            uint    territoryTypeID,
            string? mapTitle = null
        ) =>
            Unsafe.As<AgentMapExpansion, AgentMap>(ref agent).SetMapByZoneAndOpen(territoryTypeID, mapTitle);

        public void SetMapAndOpen
        (
            uint    mapID,
            string? mapTitle = null
        ) =>
            Unsafe.As<AgentMapExpansion, AgentMap>(ref agent).SetMapAndOpen(mapID, mapTitle);

        public void SetMapFlagByZoneAndOpen
        (
            uint    territoryTypeID,
            Vector3 worldPosition,
            string? mapTitle = null
        ) =>
            Unsafe.As<AgentMapExpansion, AgentMap>(ref agent).SetMapFlagByZoneAndOpen(territoryTypeID, worldPosition, mapTitle);

        public void SetMapFlagAndOpen
        (
            uint    mapID,
            Vector3 worldPosition,
            string? mapTitle = null
        ) =>
            Unsafe.As<AgentMapExpansion, AgentMap>(ref agent).SetMapFlagAndOpen(mapID, worldPosition, mapTitle);

        public void SetMapFlagAndOpen
        (
            Vector3 worldPosition,
            string? mapTitle = null
        ) =>
            Unsafe.As<AgentMapExpansion, AgentMap>(ref agent).SetMapFlagAndOpen(worldPosition, mapTitle);
    }

    extension
    (
        scoped ref AgentMap agent
    )
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

            agent.OpenMapByMapId(zoneRow.Map.RowId, territoryTypeID);
            if (mapTitle != null)
                agent.MapTitleString.SetString(mapTitle);
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

            agent.OpenMapByMapId(mapID, mapRow.TerritoryType.RowId);
            if (mapTitle != null)
                agent.MapTitleString.SetString(mapTitle);
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
            if (mapTitle != null)
                agent.MapTitleString.SetString(mapTitle);
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
            if (mapTitle != null)
                agent.MapTitleString.SetString(mapTitle);
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
