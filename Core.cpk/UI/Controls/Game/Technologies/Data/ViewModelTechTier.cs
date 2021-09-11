namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelTechTier : BaseViewModel
    {
        public const string Tier1Letter = "I";

        public const string Tier2Letter = "II";

        public const string Tier3Letter = "III";

        public const string Tier4Letter = "IV";

        public const string Tier5Letter = "V";

        // Please note: Tier 6 has a custom title. It's not about prison break or something, but rather escape from a hostile planet.
        public const string TierEscape = "Escape";

        public const string TierTitleFormat = "Tier {0}";

        public ViewModelTechTier(TechTier tier)
        {
            this.Tier = tier;

            TechGroup.AvailableTechGroupsChanged += this.AvailableTechGroupsChangedHandler;
            this.Initialize();
        }

        public List<ViewModelTechGroup> GroupsPrimary { get; private set; }

        public List<ViewModelTechGroup> GroupsSecondary { get; private set; }

        public bool HasOnlySingleTechGroup
            => this.GroupsPrimary.Count + this.GroupsSecondary.Count == 1;

        public bool IsSelected { get; set; }

        public TechTier Tier { get; }

        public string Title => GetTierText(this.Tier);

        public static string GetTierLetterOnly(TechTier tier)
        {
            return (byte)tier switch
            {
                1 => Tier1Letter,
                2 => Tier2Letter,
                3 => Tier3Letter,
                4 => Tier4Letter,
                5 => Tier5Letter,
                // not supported by the vanilla game as we don't use more than 5 tiers (Tier 6 is "Escape")
                6  => "VI",
                7  => "VII",
                8  => "VIII",
                9  => "IX",
                10 => "X",
                _  => tier.ToString()
            };
        }

        public static string GetTierText(TechTier tier)
        {
            if (tier == TechTier.Tier6)
            {
                return TierEscape;
            }

            return string.Format(TierTitleFormat, GetTierLetterOnly(tier));
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            TechGroup.AvailableTechGroupsChanged -= this.AvailableTechGroupsChangedHandler;
        }

        private void AvailableTechGroupsChangedHandler()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            var allGroups = TechGroup.AvailableTechGroups
                                     .Where(g => g.Tier == this.Tier)
                                     .OrderBy(g => g.Order)
                                     .ThenBy(g => g.ShortId)
                                     .Select(g => new ViewModelTechGroup(g))
                                     .ToList();

            this.GroupsPrimary = allGroups.Where(g => g.TechGroup.IsPrimary)
                                          .ToList();

            this.GroupsSecondary = allGroups.Where(g => !g.TechGroup.IsPrimary)
                                            .ToList();
        }
    }
}