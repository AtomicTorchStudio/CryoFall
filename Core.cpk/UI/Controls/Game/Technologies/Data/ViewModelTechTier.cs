namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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

        public const string TierTitleFormat = "Tier {0}";

        public ViewModelTechTier(TechTier tier)
        {
            this.Tier = tier;

            TechGroup.AvailableTechGroupsChanged += this.AvailableTechGroupsChangedHandler;
            this.Initialize();
        }

        public List<ViewModelTechGroup> GroupsPrimary { get; private set; }

        public List<ViewModelTechGroup> GroupsSecondary { get; private set; }

        public bool IsSelected { get; set; }

        public TechTier Tier { get; }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public string TierLetter
        {
            get
            {
                switch (this.Tier)
                {
                    case TechTier.Tier1:
                        return Tier1Letter;
                    case TechTier.Tier2:
                        return Tier2Letter;
                    case TechTier.Tier3:
                        return Tier3Letter;
                    case TechTier.Tier4:
                        return Tier4Letter;
                    case TechTier.Tier5:
                        return Tier5Letter;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string Title => string.Format(TierTitleFormat, this.TierLetter);

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