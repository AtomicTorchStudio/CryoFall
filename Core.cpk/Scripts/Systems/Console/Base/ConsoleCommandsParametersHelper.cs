namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class ConsoleCommandsParametersHelper
    {
        public static readonly string[] EmptySuggestions = new string[0];

        private static readonly Dictionary<Type, BaseConsoleCommandParameterType> ParameterTypes =
            new Dictionary<Type, BaseConsoleCommandParameterType>();

        /// <summary>
        /// Extracts segments from text.
        /// For example "FurHatCossack" will be extracted to [Fur, Hat, Cossack].
        /// </summary>
        public static List<TextSegment> GetCamelCaseSegments(string text)
        {
            text = WhitespaceToCamelCase(text);

            var result = new List<TextSegment>();
            int index = 0, lastUpperLetterIndex = 0;
            do
            {
                index++;
                var isCompleted = index == text.Length;
                if (!isCompleted)
                {
                    var letter = text[index];
                    if (!char.IsUpper(letter))
                    {
                        continue;
                    }
                }

                var segmentText = text.Substring(lastUpperLetterIndex, index - lastUpperLetterIndex);
                segmentText = segmentText.Trim();
                result.Add(new TextSegment(segmentText, lastUpperLetterIndex));
                lastUpperLetterIndex = index;

                if (isCompleted)
                {
                    break;
                }
            }
            while (true);

            return result;
        }

        public static string GetParameterDescription(Type parameterType, string parameterName)
        {
            if (!parameterType.IsEnum)
            {
                return parameterName;
            }

            var enumNames = Enum.GetNames(parameterType);
            if (enumNames.Length < 5)
            {
                // return values of enum
                return string.Join("|", enumNames);
            }

            // too many parameters - return simply the name of parameter
            return parameterName;
        }

        public static int GetSuggestionMatchIndex(
            string inputText,
            List<TextSegment> inputSegments,
            string suggestionText)
        {
            var index = suggestionText.IndexOf(inputText, StringComparison.OrdinalIgnoreCase);
            if (index >= 0
                || inputSegments.Count < 2)
            {
                return index;
            }

            // try CamelCase search
            var suggestionSegments = GetCamelCaseSegments(suggestionText);
            if (inputSegments.Count > suggestionSegments.Count)
            {
                // suggestionText contains less segments than inputText - no match
                return -1;
            }

            return TryMatchSegment(inputSegments, suggestionSegments);
        }

        public static IEnumerable<string> GetSuggestions(string startsWith, Type type)
        {
            IEnumerable<string> suggestions;

            if (!ParameterTypes.TryGetValue(type, out var value))
            {
                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = Nullable.GetUnderlyingType(type);
                }

                if (!type.IsEnum)
                {
                    return EmptySuggestions;
                }

                suggestions = Enum.GetNames(type);
            }
            else
            {
                suggestions = value.GetSuggestions();
            }

            if (string.IsNullOrEmpty(startsWith))
            {
                return suggestions;
            }

            var camelCaseSegments = GetCamelCaseSegments(startsWith);

            // filter suggestions
            // by using IndexOf we can get more usable suggestions
            return from s in suggestions
                   let index = GetSuggestionMatchIndex(startsWith, camelCaseSegments, s)
                   where index >= 0
                   orderby index
                   select s;
        }

        public static int GetTextMatchIndex(
            string searchText,
            string str)
        {
            var searchSegments = GetCamelCaseSegments(searchText);

            var index = str.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (index >= 0
                || searchSegments.Count < 2)
            {
                return index;
            }

            // try CamelCase search
            var suggestionSegments = GetCamelCaseSegments(str);
            if (searchSegments.Count > suggestionSegments.Count)
            {
                // suggestionText contains less segments than inputText - no match
                return -1;
            }

            return TryMatchSegment(searchSegments, suggestionSegments);
        }

        public static bool ParseArgument(string value, Type type, out object result)
        {
            if (ParameterTypes.TryGetValue(type, out var parser))
            {
                return parser.TryParse(value, out result);
            }

            if (type.IsPrimitive)
            {
                return TypeHelper.TryParsePrimitiveType(value, type, out result);
            }

            if (type.IsEnum)
            {
                return EnumExtensions.TryParse(type, value, out result);
            }

            if (Nullable.GetUnderlyingType(type) is not null
                && "null".Equals(value))
            {
                // parsed null
                result = null;
                return true;
            }

            // try use type converter
            var converter = TypeDescriptor.GetConverter(type);
            if (!converter.CanConvertFrom(typeof(string)))
            {
                // throw new Exception("Cannot convert parameter: " + type + " - unsupported type.");
                result = null;
                return false;
            }

            // this is wrong - from MSDN: "The IsValid method is used to validate a value within the type rather than to determine if value can be converted to the given type."
            //if (!converter.IsValid(value))
            //{
            //    return null;
            //}

            try
            {
                result = converter.ConvertFromInvariantString(value);
                return true;
            }
            catch
            {
                // cannot parse
                result = null;
                return false;
            }
        }

        public static void Register(BaseConsoleCommandParameterType consoleCommandParameterType)
        {
            ParameterTypes.Add(consoleCommandParameterType.ParameterType, consoleCommandParameterType);
        }

        private static int TryMatchSegment(
            List<TextSegment> inputSegments,
            List<TextSegment> suggestionSegments,
            int suggestionSegmentOffset = 0)
        {
            if (suggestionSegments.Count - suggestionSegmentOffset < 2)
            {
                // to short
                return -1;
            }

            var index = -1;
            for (var inputSegmentIndex = 0;
                 inputSegmentIndex < inputSegments.Count
                 && inputSegmentIndex + suggestionSegmentOffset < suggestionSegments.Count;
                 inputSegmentIndex++)
            {
                var inputSegment = inputSegments[inputSegmentIndex];
                var suggestionSegment = suggestionSegments[inputSegmentIndex + suggestionSegmentOffset];
                if (!suggestionSegment.Text.StartsWith(
                        inputSegment.Text,
                        StringComparison.OrdinalIgnoreCase))
                {
                    // suggestionSegment starts with different text
                    // try match with an offset - for example, offset==1 will match "HC" for "FurHatCossack"
                    return TryMatchSegment(inputSegments, suggestionSegments, suggestionSegmentOffset + 1);
                }

                if (index == -1)
                {
                    index = inputSegment.Index;
                }
            }

            return index;
        }

        /// <summary>
        /// Converts "Wall stone strong" -> "WallStoneStrong".
        /// </summary>
        private static string WhitespaceToCamelCase(string text)
        {
            if (text.IndexOf(' ') < 0)
            {
                // doesn't has any whitespaces
                return text;
            }

            var sb = new StringBuilder();
            var isCapitalizeNextChar = false;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c == ' ')
                {
                    isCapitalizeNextChar = true;
                }
                else
                {
                    if (isCapitalizeNextChar)
                    {
                        c = char.ToUpper(c);
                        isCapitalizeNextChar = false;
                    }

                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public struct TextSegment
        {
            public readonly int Index;

            public readonly string Text;

            public TextSegment(string text, int index)
            {
                this.Text = text;
                this.Index = index;
            }
        }
    }
}