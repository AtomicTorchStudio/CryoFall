namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.ProfanityFiltering;

    public static class ClanTagFilterHelper
    {
        private static readonly IReadOnlyList<string> BlockList
            = new[]
            {
                "DEV*",
                "ADM*",
                "GM*",
                "MOD*",
                "CLAN"
            };

        public static bool ContainsProfanityOrProhibited(string clanTag)
        {
            if (string.IsNullOrEmpty(clanTag))
            {
                return true;
            }

            clanTag = ProfanityFilteringSystem.SharedApplyFilters(clanTag);
            if (clanTag.IndexOf(ProfanityFilteringSystem.ProfanityFilterReplacementChar)
                >= 0)
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