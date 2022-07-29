namespace AtomicTorch.CBND.CoreMod.Systems.ProfanityFiltering
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public static class ProfanityFilteringSystem
    {
        public const char ProfanityFilterReplacementChar = '*';

        private static readonly List<FilterEntry> FilterBlacklist = new();

        private static readonly List<FilterEntry> FilterWhitelist = new();

        private static readonly char[] TrimWildcardChar = { '*' };

        public static string SharedApplyFilters(string message)
        {
            if (message is null)
            {
                return null;
            }

            var originalMessage = message;

            if (Api.IsClient
                && Api.Client.ExternalApi.IsExternalClient)
            {
                message = Api.Client.ExternalApi.FilterText(message);
            }

            foreach (var test in FilterBlacklist)
            {
                var startIndex = 0;
                do
                {
                    var currentIndex = message.IndexOf(test.Text,
                                                       startIndex,
                                                       StringComparison.OrdinalIgnoreCase);
                    if (currentIndex < 0)
                    {
                        // try next test
                        break;
                    }

                    startIndex = currentIndex + test.Text.Length;

                    if (TestExclusion(message, test, currentIndex)
                        || IsWhitelistExclusion(currentIndex, test.Text.Length))
                    {
                        continue;
                    }

                    // censor entry
                    message = Censor(message,
                                     currentIndex,
                                     test.Text.Length,
                                     out var extraCharsCensoredCount);
                    startIndex += extraCharsCensoredCount;
                }
                while (true);
            }

            if (originalMessage != message)
            {
                Api.Logger.Important("Filtered profanity: "
                                     + Environment.NewLine
                                     + originalMessage
                                     + Environment.NewLine
                                     + "->"
                                     + Environment.NewLine
                                     + message,
                                     characterRelated: Api.IsServer && ServerRemoteContext.IsRemoteCall
                                                           ? ServerRemoteContext.Character
                                                           : null);
            }

            return message;

            bool IsWhitelistExclusion(int index, int length)
            {
                foreach (var test in FilterWhitelist)
                {
                    var searchCount = Math.Max(test.Text.Length, length);
                    if (index + searchCount > message.Length)
                    {
                        continue;
                    }

                    if (message.IndexOf(test.Text,
                                        index,
                                        count: searchCount,
                                        StringComparison.OrdinalIgnoreCase)
                        != index)
                    {
                        continue;
                    }

                    if (TestExclusion(message, test, index))
                    {
                        continue;
                    }

                    // whitelist entry matched
                    return true;
                }

                return false;
            }

            string Censor(string text, int currentIndex, int length, out int extraCharsCensoredCount)
            {
                // find the end of the censored word
                var endIndex = FindNextEmptyCharIndex(text,
                                                      currentIndex + length);
                if (endIndex > 0)
                {
                    var newLength = endIndex - currentIndex;
                    extraCharsCensoredCount = newLength - length;
                    length = newLength;
                }
                else
                {
                    extraCharsCensoredCount = 0;
                }

                // find the beginning of the censored word
                var newStartIndex = FindFirstNonEmptyCharIndex(text,
                                                               currentIndex);
                var deltaIndex = currentIndex - newStartIndex;
                length += deltaIndex;
                currentIndex = newStartIndex;

                return text.Remove(currentIndex, length)
                           .Insert(currentIndex, new string(ProfanityFilterReplacementChar, length));
            }

            int FindNextEmptyCharIndex(string text, int index)
            {
                for (; index < text.Length; index++)
                {
                    if (SharedIsEmptyChar(text[index]))
                    {
                        return index;
                    }
                }

                return text.Length;
            }

            int FindFirstNonEmptyCharIndex(string text, int index)
            {
                for (; index >= 0; index--)
                {
                    if (SharedIsEmptyChar(text[index]))
                    {
                        return index + 1;
                    }
                }

                return 0;
            }
        }

        private static bool SharedIsEmptyChar(char c)
        {
            return !char.IsLetterOrDigit(c);
        }

        private static bool TestExclusion(string message, FilterEntry test, int index)
        {
            if (!test.AllowsPrefix
                && index > 0
                && !SharedIsEmptyChar(message[index - 1]))
            {
                // prefix check failed
                return true;
            }

            if (!test.AllowsSuffix)
            {
                var nextIndex = index + test.Text.Length;
                if (nextIndex < message.Length
                    && !SharedIsEmptyChar(message[nextIndex]))
                {
                    // suffix check failed
                    return true;
                }
            }

            return false;
        }

        [NotPersistent]
        private readonly struct FilterEntry
        {
            public readonly bool AllowsPrefix;

            public readonly bool AllowsSuffix;

            public readonly string Text;

            private readonly string OriginalEntry;

            public FilterEntry(string originalEntry, string text, bool allowsPrefix, bool allowsSuffix)
            {
                this.OriginalEntry = originalEntry;
                this.Text = text;
                this.AllowsPrefix = allowsPrefix;
                this.AllowsSuffix = allowsSuffix;
            }

            public override string ToString()
            {
                return this.OriginalEntry;
            }

            internal static int FilterSortComparison(FilterEntry a, FilterEntry b)
            {
                var compareLength = b.OriginalEntry.Length.CompareTo(
                    a.OriginalEntry.Length);
                if (compareLength != 0)
                {
                    return compareLength;
                }

                return string.Compare(a.Text, b.Text, StringComparison.Ordinal);
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Initialize();
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                Initialize();
            }

            private static void Initialize()
            {
                var sb = new StringBuilder();
                foreach (var filePath in Api.Shared.FindFiles("Scripts/Systems/ProfanityFiltering/Filters/")
                                            .EnumerateAndDispose())
                {
                    if (!filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var filterFileContent = Api.Shared.LoadTextFileContent(filePath);
                    LoadFilters(filterFileContent);
                }

                FilterBlacklist.Sort(FilterEntry.FilterSortComparison);
                FilterWhitelist.Sort(FilterEntry.FilterSortComparison);

                Api.Logger.Important(
                    $"Profanity filtering lists loaded: {FilterBlacklist.Count} blacklist entries, {FilterWhitelist.Count} whitelist entries");

                void LoadFilters(string filterFileContent)
                {
                    sb.Clear();
                    foreach (var c in filterFileContent)
                    {
                        switch (c)
                        {
                            case '\r':
                                // skip
                                break;

                            case '\n':
                                // newline
                                CommitEntry();
                                break;

                            default:
                                sb.Append(c);
                                break;
                        }
                    }

                    CommitEntry();

                    void CommitEntry()
                    {
                        if (sb.Length == 0)
                        {
                            return;
                        }

                        try
                        {
                            if (sb[0] == '#')
                            {
                                return;
                            }

                            var isWhitelistEntry = sb[0] == '-';
                            var originalEntry = isWhitelistEntry
                                                    ? sb.ToString(1, sb.Length - 1)
                                                    : sb.ToString();

                            var text = originalEntry;
                            var allowsPrefix = text[0] == '*';
                            var allowsSuffix = text[text.Length - 1] == '*';
                            if (allowsPrefix || allowsSuffix)
                            {
                                text = text.Trim(TrimWildcardChar);
                            }

                            var entry = new FilterEntry(originalEntry, text, allowsPrefix, allowsSuffix);
                            var list = isWhitelistEntry
                                           ? FilterWhitelist
                                           : FilterBlacklist;

                            list.Add(entry);
                        }
                        finally
                        {
                            sb.Clear();
                        }
                    }
                }
            }
        }
    }
}