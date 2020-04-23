namespace AtomicTorch.CBND.CoreMod.Systems.Party
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.ProfanityFiltering;

    public static class ClanTagFilterHelper
    {
        private static readonly IReadOnlyList<string> BlockList
            = new[]
            {
                "DEV*",
                "ADM*",
                "GM*",
                "MOD*"
            };

        public static bool IsInvalidClanTag(string clanTag)
        {
            if (string.IsNullOrEmpty(clanTag))
            {
                return false;
            }

            clanTag = ProfanityFilteringSystem.SharedApplyFilters(clanTag);
            if (clanTag.Contains(ProfanityFilteringSystem.ProfanityFilterReplacementChar))
            {
                // found profanity
                return true;
            }

            foreach (var entry in BlockList)
            {
                if (entry[entry.Length - 1] == '*')
                {
                    if (clanTag.StartsWith(entry.Substring(0, entry.Length - 1), StringComparison.OrdinalIgnoreCase))
                    {
                        // matched a block list entry with wildcard
                        return true;
                    }
                }
                else if (clanTag.Equals(entry))
                {
                    // matched a block list entry
                    return true;
                }
            }

            return false;
        }
    }
}