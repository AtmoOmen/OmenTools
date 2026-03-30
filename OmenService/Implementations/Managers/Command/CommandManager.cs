using System.Collections.Concurrent;
using System.Text;
using Dalamud.Game.Command;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using OmenTools.Dalamud;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public sealed class CommandManager : OmenServiceBase<CommandManager>
{
    /// <remarks>需要以半角斜杠为前缀</remarks>
    public MainCommandInfo? MainCommand
    {
        get;
        set
        {
            if (value == null)
            {
                if (field != null)
                    RemoveCommand(field.Command);
                
                return;
            }
            
            field = value;
            
            RefreshCommandDetails();
        }
    }

    public ConcurrentDictionary<string, CommandInfo> Commands    { get; private set; } = [];
    public ConcurrentDictionary<string, CommandInfo> SubCommands { get; private set; } = [];

    protected override void Uninit()
    {
        foreach (var command in Commands.Keys)
            RemoveCommand(command);

        Commands.Clear();
        SubCommands.Clear();

        MainCommand = null;
    }

    private unsafe void OnMainCommand(string command, string args)
    {
        DLog.Debug($"[CommandManager] [MainCommand] {command} {args}");
        
        if (string.IsNullOrEmpty(args))
        {
            MainCommand.Info.Handler(command, args);
            return;
        }

        var splitedArgs = args.Split(' ', 2);
        if (SubCommands.TryGetValue(splitedArgs[0], out var commandInfo))
            commandInfo.Handler(splitedArgs[0], splitedArgs.Length > 1 ? splitedArgs[1] : string.Empty);
        else
            NotifyCommandError();

        return;

        void NotifyCommandError()
        {
            using var utf8String = new Utf8String($"{command} {args}");
            RaptureLogModule.Instance()->ShowLogMessageString(725, &utf8String);
        }
    }

    public bool AddCommand(string command, CommandInfo commandInfo, bool isForceToAdd = false)
    {
        if (!isForceToAdd && DService.Instance().Command.Commands.ContainsKey(command)) return false;

        RemoveCommand(command);
        DService.Instance().Command.AddHandler(command, commandInfo);
        Commands[command] = commandInfo;

        return true;
    }

    public bool AddSubCommand(string subCommand, CommandInfo commandInfo, bool isForceToAdd = false)
    {
        if (!isForceToAdd && SubCommands.ContainsKey(subCommand)) return false;

        SubCommands[subCommand] = commandInfo;
        RefreshCommandDetails();
        return true;
    }

    public bool RemoveCommand(string command)
    {
        if (!DService.Instance().Command.Commands.ContainsKey(command))
            return false;

        DService.Instance().Command.RemoveHandler(command);
        Commands.TryRemove(command, out _);
        return true;
    }

    public bool RemoveSubCommand(string subCommand)
    {
        if (!SubCommands.TryRemove(subCommand, out _))
            return false;

        RefreshCommandDetails();
        return true;
    }

    private void RefreshCommandDetails()
    {
        if (MainCommand == null)
            return;

        var helpMessage = new StringBuilder($"{MainCommand.Info.HelpMessage}\n");
        foreach (var (command, commandInfo) in SubCommands.Where(x => x.Value.ShowInHelp))
            helpMessage.AppendLine($"{MainCommand.Command} {command} -> {commandInfo.HelpMessage}");

        RemoveCommand(MainCommand.Command);
        AddCommand
        (
            MainCommand.Command,
            new(OnMainCommand)
            {
                HelpMessage  = helpMessage.ToString(),
                ShowInHelp   = MainCommand.Info.ShowInHelp,
                DisplayOrder = MainCommand.Info.DisplayOrder
            },
            true
        );
    }
}
