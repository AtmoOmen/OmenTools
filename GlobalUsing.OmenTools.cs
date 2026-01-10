// ReSharper disable RedundantUsingDirective.Global

#region OmenTools

global using OmenTools;
global using OmenTools.Infos;
global using OmenTools.Service;
global using OmenTools.Managers;
global using OmenTools.ImGuiOm;
global using OmenTools.Helpers;
global using OmenTools.Extensions;
global using OmenTools.Widgets;

global using IAetheryteList = OmenTools.Service.IAetheryteList;
global using IAetheryteEntry = OmenTools.Service.IAetheryteEntry;
global using IPlayerCharacter = OmenTools.Service.IPlayerCharacter;
global using ICharacter = OmenTools.Service.ICharacter;
global using IGameObject = OmenTools.Service.IGameObject;
global using IObjectTable = OmenTools.Service.IObjectTable;
global using IEventObj = OmenTools.Service.IEventObj;
global using INPC = OmenTools.Service.INPC;
global using IBattleChara = OmenTools.Service.IBattleChara;
global using IBattleNPC = OmenTools.Service.IBattleNPC;
global using StatusList = OmenTools.Service.StatusList;

global using static OmenTools.Helpers.HelpersOm;
global using static OmenTools.Infos.InfosOm;
global using static OmenTools.Helpers.ThrottlerHelper;
global using static OmenTools.Infos.GameAddon;

#endregion

#region Dalamud

global using Dalamud.Bindings.ImGui;
global using Dalamud.Bindings.ImGuizmo;
global using Dalamud.Bindings.ImPlot;
global using Dalamud.Interface;
global using Dalamud.Interface.Utility.Raii;
global using Dalamud.Game;

#endregion

#region C#

global using System.Drawing;

#endregion
