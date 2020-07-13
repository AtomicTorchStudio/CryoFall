namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelSkillEffectCombinedStats : BaseViewModelSkillEffect
    {
        public const string NextLevelPrefix = "next level";

        private readonly StatEffect[] effects;

        private readonly IProtoSkill skill;

        public ViewModelSkillEffectCombinedStats(IProtoSkill skill, StatEffect[] effects, byte maxLevel) : base(
            level: 0,
            maxLevel: maxLevel)
        {
            this.skill = skill;
            this.effects = effects;
        }

        public override void Refresh(byte currentLevel)
        {
            this.Visibility = Visibility.Visible;

            double totalValueBonusNow = 0,
                   totalPercentBonusNow = 0,
                   totalValueBonusNextLevel = 0,
                   totalPercentBonusNextLevel = 0;

            var nextLevel = (byte)(currentLevel + 1);
            if (nextLevel > this.MaxLevel)
            {
                nextLevel = this.MaxLevel;
            }

            var hasNextLevel = nextLevel != currentLevel;

            foreach (var statEffect in this.effects)
            {
                if (statEffect.Level > currentLevel)
                {
                    continue;
                }

                totalValueBonusNow += statEffect.CalcTotalValueBonus(currentLevel);
                totalPercentBonusNow += statEffect.CalcTotalPercentBonus(currentLevel);

                if (hasNextLevel)
                {
                    totalValueBonusNextLevel += statEffect.CalcTotalValueBonus(nextLevel);
                    totalPercentBonusNextLevel += statEffect.CalcTotalPercentBonus(nextLevel);
                }
            }

            if (totalValueBonusNow == 0d
                && totalPercentBonusNow == 0d)
            {
                this.IsActive = false;
                this.Visibility = Visibility.Collapsed;
                return;
            }

            this.IsActive = true;
            this.Visibility = Visibility.Visible;

            var firstEffect = this.effects[0];
            var displayTotalValue = firstEffect.DisplayTotalValue;

            var sb = new AutoStringBuilder(firstEffect.Description);
            sb.Append(" ");

            double globalValue = 0,
                   globalPercentValue = 0;

            if (displayTotalValue)
            {
                var statName = firstEffect.StatName;
                var cache = ClientCurrentCharacterHelper.Character.SharedGetFinalStatsCache();
                foreach (var statEntry in cache.Sources.List)
                {
                    if (statEntry.StatName == statName
                        && !ReferenceEquals(statEntry.Source, this.skill))
                    {
                        globalValue += statEntry.Value;
                        globalPercentValue += statEntry.Percent;
                    }
                }
            }

            ViewModelSkillEffectStat.FormatBonusText(sb,
                                                     globalValue + totalValueBonusNow,
                                                     globalPercentValue + totalPercentBonusNow,
                                                     canDisplayPositiveSign: !displayTotalValue);

            if (hasNextLevel
                && (totalValueBonusNextLevel != totalValueBonusNow
                    || totalPercentBonusNextLevel != totalPercentBonusNow))
            {
                sb.Append(" (")
                  .Append(NextLevelPrefix)
                  .Append(" ");

                ViewModelSkillEffectStat.FormatBonusText(sb,
                                                         globalValue + totalValueBonusNextLevel,
                                                         globalPercentValue + totalPercentBonusNextLevel,
                                                         canDisplayPositiveSign: !displayTotalValue);
                sb.Append(")");
            }

            this.Description = sb;
        }
    }
}