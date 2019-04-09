namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeBool : BaseConsoleCommandParameterType
    {
        private static readonly string[] Suggestions =
        {
            "1",
            "0"
        };

        public override Type ParameterType { get; } = typeof(bool);

        public override string ShortDescription => "1 or 0, yes or no, true or false";

        public override IEnumerable<string> GetSuggestions()
        {
            return Suggestions;
        }

        public override bool TryParse(string value, out object result)
        {
            if (string.IsNullOrEmpty(value))
            {
                // cannot parse
                result = null;
                return false;
            }

            value = value.Trim();
            if (value == "1"
                || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "yes",  StringComparison.OrdinalIgnoreCase))
            {
                // parsed: True
                result = true;
                return true;
            }

            if (value == "0"
                || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "no",    StringComparison.OrdinalIgnoreCase))
            {
                // parsed: False
                result = false;
                return true;
            }

            // cannot parse
            result = null;
            return false;
        }
    }
}