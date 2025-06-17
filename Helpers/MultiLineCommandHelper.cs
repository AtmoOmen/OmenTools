namespace OmenTools.Helpers;

public static class MultiLineCommandHelper
{
    public static async Task SendMultiLineCommand(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            DService.Log.Error("待发送消息为空");
            return;
        }

        var commands = message.Split('\n')
                              .Select(c => c.Trim())
                              .Where(c => !string.IsNullOrWhiteSpace(c));

        foreach (var command in commands)
        {
            try
            {
                if (TryParseWaitCommand(command, out var delayMs))
                {
                    await Task.Delay(delayMs);
                    continue;
                }

                ChatHelper.SendMessage(command);
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                DService.Log.Error($"执行命令时出错: {command}\n错误: {ex.Message}");
            }
        }
    }
    
    private static bool TryParseWaitCommand(string command, out int delayMs)
    {
        delayMs = 0;
        if (!command.StartsWith("/wait ", StringComparison.OrdinalIgnoreCase)) return false;

        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1 && float.TryParse(parts[1], out var waitTime))
        {
            delayMs = (int)(waitTime * 1000);
            return true;
        }
        return false;
    }
}
