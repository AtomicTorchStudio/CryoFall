namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.ProfanityFiltering;

    public static class SharedTextHelper
    {
        public static string TrimAndFilterProfanity(string text)
        {
            text = text?.Trim() ?? string.Empty;
            text = TrimSpacesOnEachLine(text);
            text = ProfanityFilteringSystem.SharedApplyFilters(text);
            return text;
        }

        public static string TrimSpacesOnEachLine(string text)
        {
            text = text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            text = text.Replace("\r", "");
            var split = text.Split('\n');
            for (var index = 0; index < split.Length; index++)
            {
                var line = split[index];
                line = line.Trim();
                split[index] = line;
            }

            text = string.Join(Environment.NewLine, split);
            return text;
        }
    }
}