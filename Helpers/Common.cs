namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static unsafe string UTF8StringToString(Utf8String str)
    {
        if (str.StringPtr == null || str.BufUsed <= 1)
            return string.Empty;
        return Encoding.UTF8.GetString(str.StringPtr, (int)str.BufUsed - 1);
    }
}