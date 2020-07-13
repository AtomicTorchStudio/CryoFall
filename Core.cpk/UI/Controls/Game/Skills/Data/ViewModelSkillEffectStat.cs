namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelSkillEffectStat : BaseViewModelSkillEffect
    {
        public ViewModelSkillEffectStat(StatEffect statEffect, byte maxLevel) : base(statEffect, maxLevel)
        {
            this.StatEffect = statEffect;
        }

        public StatEffect StatEffect { get; }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static void FormatBonusText(
            AutoStringBuilder text,
            double valueBonus,
            double percentBonus,
            bool canDisplayPositiveSign)
        {
            var hasValueBonus = false;
            if (valueBonus != 0)
            {
                hasValueBonus = true;

                if (canDisplayPositiveSign
                    || valueBonus < 0)
                {
                    text.Append(SignChar(valueBonus));
                }

                text.Append(valueBonus.ToString("0.##"));
            }

            if (percentBonus == 0)
            {
                return;
            }

            if (hasValueBonus)
            {
                text.Append(" (");
            }

            if (canDisplayPositiveSign
                || percentBonus < 0)
            {
                text.Append(SignChar(percentBonus));
            }

            text.Append(percentBonus.ToString("0.##"))
                .Append("%");

            if (hasValueBonus)
            {
                text.Append(")");
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
            var text = (AutoStringBuilder)this.StatEffect.Description;

            var level = currentLevel;
            if (level < this.Level)
            {
                level = this.Level;
            }

            // TODO: we didn't implement handling of StatEffect.DisplayTotalValue here, it's just used to hide the + sign
            // it's implemented only for ViewModelSkillEffectCombinedStats
            var totalValueBonus = this.StatEffect.CalcTotalValueBonus(level);
            var totalPercentBonus = this.StatEffect.CalcTotalPercentBonus(level);

            text.Append(" ");
            FormatBonusText(text,
                            totalValueBonus,
                            totalPercentBonus,
                            canDisplayPositiveSign: !this.StatEffect.DisplayTotalValue);

            this.Description = text;
        }

        private static string SignChar(double totalValueBonus)
        {
            return totalValueBonus > 0 ? "+" : "";
        }
    }
}