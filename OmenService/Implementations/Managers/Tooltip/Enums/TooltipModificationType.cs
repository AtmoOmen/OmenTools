namespace OmenTools.OmenService;

/// <summary>
///     Tooltip 文本修改类型。
///     <para>
///         一个 tooltip 文本在渲染时会被分为三块:
///     </para>
///     <list type="bullet">
///         <item>前置信息区</item>
///         <item>主体信息区</item>
///         <item>后置信息区</item>
///     </list>
/// </summary>
public enum TooltipModificationType
{
    /// <summary>
    ///     向主体信息区贡献一段内容。
    /// </summary>
    /// <remarks>
    ///     当至少存在一个 <see cref="Contribute" /> 修改时，主体信息区将由所有贡献内容组合而成，
    ///     不再显示游戏原始文本。
    ///     <para>
    ///         若不存在任何 <see cref="Contribute" /> 修改，则主体信息区显示游戏原始文本。
    ///     </para>
    /// </remarks>
    Contribute,

    /// <summary>
    ///     向前置信息区添加一段内容。
    /// </summary>
    /// <remarks>
    ///     前置信息区会显示在主体信息区之前。
    /// </remarks>
    Prepend,

    /// <summary>
    ///     向后置信息区添加一段内容。
    /// </summary>
    /// <remarks>
    ///     后置信息区会显示在主体信息区之后。
    /// </remarks>
    Append
}
