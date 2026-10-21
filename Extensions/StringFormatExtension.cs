using Jeffijoe.MessageFormat;
using Lumina.Data;

namespace OmenTools.Extensions;

public static class StringFormatExtension
{
    extension
    (
        string
    )
    {
        public static string FormatICUText
        (
            string                              value,
            IReadOnlyDictionary<string, object> args,
            Language?                           language      = null,
            Action<Exception>?                  onFormatError = null
        )
        {
            ArgumentNullException.ThrowIfNull(args);

            try
            {
                return MessageFormatter.Format(value, args, language?.ToCultureInfo());
            }
            catch (Exception ex)
            {
                onFormatError?.Invoke(ex);
                return value;
            }
        }
    }
}
