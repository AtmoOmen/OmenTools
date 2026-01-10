using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Infos;

public static unsafe class AgentLobbyEvent
{
    public static void SelectCharacterByIndex(uint index)
    {
        AgentId.Lobby.SendEvent(0, 21, (int)index);
        AgentId.Lobby.SendEvent(3, 0);
    }

    public static bool SelectCharacter(Predicate<CharaSelectCharacterEntry> condition)
    {
        if (condition == null) return false;
        
        var agent = AgentLobby.Instance();
        if (agent == null) return false;

        var entries = agent->LobbyData.CharaSelectEntries;
        if (entries.Count == 0) return  false;

        for (var index = 0; index < entries.Count; index++)
        {
            var entryPtr = entries[index];
            if (entryPtr == null || entryPtr.Value == null) continue;
            
            var entry = entryPtr.Value;
            if (condition(*entry))
            {
                SelectCharacterByIndex((uint)index);
                return true;
            }
        }

        return false;
    }

    public static void SelectWorldByIndex(uint index) =>
        AgentId.Lobby.SendEvent(0, 25, 0, (int)index);
    
    public static bool SelectWorldByName(string name)
    {
        name = name.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(name)) return false;
        
        var stringArray = AtkStage.Instance()->GetStringArrayData(StringArrayType.CharaSelect)->StringArray;
        for (var i = 0; i < 8; i++)
        {
            var worldNamePtr = stringArray[i];
            if (!worldNamePtr.HasValue || worldNamePtr.Value == null) continue;

            var worldName = worldNamePtr.ToString().Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(worldName)) continue;

            if (worldName.Contains(name))
            {
                SelectWorldByIndex((uint)i);
                return true;
            }
        }
        
        return false;
    }

    public static bool SelectWorldByID(uint worldID) =>
        SelectWorldByName(LuminaWrapper.GetWorldName(worldID));

    public static void OpenWorldSelect() =>
        AgentId.Lobby.SendEvent(0, 36);
    
    public static void OpenCharacterCreation() =>
        AgentId.Lobby.SendEvent(5, 7);
    
    public static void CloseCharacterCreation() =>
        AgentId.Lobby.SendEvent(0, 22);

    public static void CloseCharacterSelect() =>
        AgentId.Lobby.SendEvent(0, 19);
    
    public static void OpenCharacterSelect() =>
        AgentId.Lobby.SendEvent(0, 4);
}
