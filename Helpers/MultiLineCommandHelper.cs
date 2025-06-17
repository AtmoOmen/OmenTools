namespace OmenTools.Helpers;

public static class MultiLineCommandHelper
{
    public static async Task SendMultiLineCommandAsync(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            DService.Log.Error("待发送消息为空");
            return;
        }

        foreach (var command in message.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(command))
                continue;

            try
            {
                if (command.Trim().StartsWith("/wait ") &&
                    float.TryParse(command.Trim().Split(' ')[1], out var waitTime))
                {
                    await Task.Delay((int) waitTime * 1000);
                    continue;
                }

                ChatHelper.SendMessage(command);
                await Task.Delay(50);  //每条命令之间有足够间隔，以确保顺序执行
            }
            catch (Exception ex)
            {
                DService.Log.Error($"执行命令时出错: {command}\n错误: {ex.Message}");
            }
        }
    }
}
