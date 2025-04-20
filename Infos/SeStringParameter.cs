using Lumina.Text.Expressions;
using Lumina.Text.ReadOnly;
using DSeString = Dalamud.Game.Text.SeStringHandling.SeString;
using LSeString = Lumina.Text.SeString;

namespace OmenTools.Infos;

public readonly struct SeStringParameter
{
    private readonly uint             num;
    private readonly ReadOnlySeString str;

    public bool             IsString    { get; }
    public uint             UIntValue   => IsString ? uint.TryParse(str.ExtractText(), out var value) ? value : 0 : num;
    public ReadOnlySeString StringValue => IsString ? str : new ReadOnlySeString(num.ToString());

    public SeStringParameter(uint value) => num = value;

    public SeStringParameter(ReadOnlySeString value)
    {
        str      = value;
        IsString = true;
    }

    public SeStringParameter(string value)
    {
        str      = new ReadOnlySeString(value);
        IsString = true;
    }

    public static implicit operator SeStringParameter(int value) => new((uint)value);

    public static implicit operator SeStringParameter(uint value) => new(value);

    public static implicit operator SeStringParameter(ReadOnlySeString value) => new(value);

    public static implicit operator SeStringParameter(ReadOnlySeStringSpan value) => new(new ReadOnlySeString(value));

    public static implicit operator SeStringParameter(LSeString value) => new(new ReadOnlySeString(value.RawData));

    public static implicit operator SeStringParameter(DSeString value) => new(new ReadOnlySeString(value.Encode()));

    public static implicit operator SeStringParameter(string value) => new(value);

    public static SeStringParameter[] GetLocalParameters(ReadOnlySeStringSpan span)
    {
        Dictionary<uint, SeStringParameter> parameters = [];

        ProcessString(span);

        if (parameters.Count > 0)
        {
            var last = parameters.OrderBy(x => x.Key).Last();

            if (parameters.Count != last.Key)
            {
                for (var i = 1u; i <= last.Key; i++)
                {
                    if (!parameters.ContainsKey(i))
                        parameters[i] = new SeStringParameter(0);
                }
            }
        }

        return parameters.OrderBy(x => x.Key).Select(x => x.Value).ToArray();

        void ProcessExpression(ReadOnlySeExpressionSpan expression)
        {
            while (true)
            {
                if (expression.TryGetString(out var exprString))
                {
                    ProcessString(exprString);
                    return;
                }

                if (expression.TryGetBinaryExpression(out var expressionType, out var operand1, out var operand2))
                {
                    ProcessExpression(operand1);
                    expression = operand2;
                    continue;
                }

                if (expression.TryGetParameterExpression(out expressionType, out var operand))
                {
                    if (!operand.TryGetUInt(out var index)) return;

                    if (parameters.ContainsKey(index)) return;

                    switch (expressionType)
                    {
                        case (int)ExpressionType.LocalNumber:
                            parameters[index] = new SeStringParameter(0);
                            return;
                        case (int)ExpressionType.LocalString:
                            parameters[index] = new SeStringParameter(string.Empty);
                            return;
                    }
                }

                break;
            }
        }

        void ProcessString(ReadOnlySeStringSpan readOnlySeStringSpan)
        {
            foreach (var payload in readOnlySeStringSpan)
            {
                foreach (var expression in payload)
                    ProcessExpression(expression);
            }
        }
    }
}
