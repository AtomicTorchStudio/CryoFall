namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelSkillEffectCombinedStats : BaseViewModelSkillEffect
    {
        public const string NextLevelPrefix = "next level";

        private readonly StatEffect[] effects;

        public ViewModelSkillEffectCombinedStats(StatEffect[] effects, byte maxLevel) : base(
            level: 0,
            maxLevel: maxLevel)
        {
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

            var text = new AutoStringBuilder(this.effects[0].Description);
            text.Append(" ");
            ViewModelSkillEffectStat.FormatBonusText(text, totalValueBonusNow, totalPercentBonusNow);

            if (totalValueBonusNextLevel != 0
                || totalPercentBonusNextLevel != 0)
            {
                text.Append(" (")
                    .Append(NextLevelPrefix)
                    .Append(" ");

                ViewModelSkillEffectStat.FormatBonusText(text,
                                                         totalValueBonusNextLevel,
                                                         totalPercentBonusNextLevel);
                text.Append(")");
            }

            this.Description = text;
        }
    }
}