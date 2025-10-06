namespace OmenTools.Infos;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IPCSubscriberAttribute(string ipcName) : Attribute
{
    public string  IPCName        { get; } = ipcName;
    public bool    AutoInitialize { get; set; } = true;
    
    /// <summary>
    ///     默认值（字符串表示），将根据目标类型进行转换
    /// </summary>
    public string? DefaultValue { get; set; }
    
    /// <summary>
    ///     验证属性配置是否有效
    /// </summary>
    /// <returns>验证结果和错误信息</returns>
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(IPCName))
            return (false, "IPC名称不能为空");
        
        return (true, null);
    }

    /// <summary>
    ///     获取类型化的默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>转换后的默认值</returns>
    public T? GetTypedDefaultValue<T>()
    {
        if (string.IsNullOrEmpty(DefaultValue))
            return default;

        try
        {
            var targetType = typeof(T);

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                targetType = Nullable.GetUnderlyingType(targetType)!;

            return targetType.Name switch
            {
                nameof(Boolean) => (T)(object)bool.Parse(DefaultValue),
                nameof(Int32)   => (T)(object)int.Parse(DefaultValue),
                nameof(Int64)   => (T)(object)long.Parse(DefaultValue),
                nameof(Single)  => (T)(object)float.Parse(DefaultValue),
                nameof(Double)  => (T)(object)double.Parse(DefaultValue),
                nameof(String)  => (T)(object)DefaultValue,
                _               => (T)Convert.ChangeType(DefaultValue, targetType)
            };
        }
        catch
        {
            return default;
        }
    }
}
