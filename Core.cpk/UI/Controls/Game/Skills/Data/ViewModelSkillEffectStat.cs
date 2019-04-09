namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelSkillEffectStat : BaseViewModelSkillEffect
    {
        private readonly StatEffect statEffect;

        public ViewModelSkillEffectStat(StatEffect statEffect, byte maxLevel) : base(statEffect, maxLevel)
        {
            this.statEffect = statEffect;
        }

        public StatEffect StatEffect => this.statEffect;

        public static void FormatBonusText(AutoStringBuilder text, double totalValueBonus, double totalPercentBonus)
        {
            if (totalValueBonus != 0)
            {
                text.Append(SignChar(totalValueBonus))
                    .Append(totalValueBonus.ToString("0.##"));
            }

            if (totalPercentBonus != 0)
            {
                text.Append(SignChar(totalPercentBonus))
                    .Append(totalPercentBonus.ToString("0.##"))
                    .Append("%");
            }
        }

        public override void Refresh(byte currentLevel)
        {
            this.IsActive = currentLevel >= this.Level;
            if (this.IsActive)
            {
                // for active stat effect we're using combined ViewModelSkillEffectCombinedStats
                this.Visibility = Visibility.Collapsed;
                return;
            }

            this.Visibility = Visibility.Visible;
            var text = (AutoStringBuilder)this.statEffect.Description;

            var level = currentLevel;
            if (level < this.Level)
            {
                level = this.Level;
            }

            var totalValueBonus = this.statEffect.CalcTotalValueBonus(level);
            var totalPercentBonus = this.statEffect.CalcTotalPercentBonus(level);

            text.Append(" ");
            FormatBonusText(text, totalValueBonus, totalPercentBonus);

            this.Description = text;
        }

        private static string SignChar(double totalValueBonus)
        {
            return totalValueBonus > 0 ? "+" : "";
        }
    }
}