namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.StatModificationDisplay.Data
{
    using System.Text;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.GameEngine.Common.Extensions;

    [NotPersistent]
    public class StatModificationData
    {
        public StatModificationData(StatName statName, double value, double percent)
        {
            this.StatName = statName;
            this.Value = value;
            this.Percent = percent;
        }

        public string EffectText
        {
            get
            {
                var sb = new StringBuilder();
                ViewModelSkillEffectStat.FormatBonusText(sb,
                                                         this.Value,
                                                         (this.Percent - 1.0) * 100,
                                                         canDisplayPositiveSign: true);
                return sb.ToString();
            }
        }

        public Visibility EffectTextVisibility
            => this.StatName.GetAttribute<StatValueHiddenAttribute>() is null
                   ? Visibility.Visible
                   : Visibility.Collapsed;

        public bool IsNegativeEffect
        {
            get
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (this.Percent != 1.0)
                {
                    if (this.StatName.GetAttribute<StatDebuffAttribute>() is not null)
                    {
                        // display in red color if the multiplier is above 1
                        return this.Percent > 1.0;
                    }

                    if (this.StatName.GetAttribute<StatBuffAttribute>() is not null)
                    {
                        // display in red color if the multiplier is below 1
                        return this.Percent < 1.0;
                    }
                }
                else
                {
                    if (this.StatName.GetAttribute<StatDebuffAttribute>() is not null)
                    {
                        // display in red color if the multiplier is above 0
                        return this.Value > 0;
                    }

                    if (this.StatName.GetAttribute<StatBuffAttribute>() is not null)
                    {
                        // display in red color if the multiplier is below 0
                        return this.Value < 0;
                    }
                }

                return false;
            }
        }

        public double Percent { get; set; }

        public StatName StatName { get; }

        public string StatTitle => SharedCharacterStatsHelper.GetFullStatTitle(this.StatName);

        public double Value { get; set; }
    }
}