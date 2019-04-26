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

        public static void FormatBonusText(AutoStringBuilder text, double value, double percent)
        {
            var hasValueBonus = false;
            if (value != 0)
            {
                hasValueBonus = true;
                text.Append(SignChar(value))
                    .Append(value.ToString("0.##"));
            }

            if (percent != 0)
            {
                if (hasValueBonus)
                {
                    text.Append(" (");
                }

                text.Append(SignChar(percent))
                    .Append(percent.ToString("0.##"))
                    .Append("%");

                if (hasValueBonus)
                {
                    text.Append(")");
                }
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