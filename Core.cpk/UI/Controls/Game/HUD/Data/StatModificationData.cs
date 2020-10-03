namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
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
            => this.StatName.GetAttribute<StatNameHiddenValueAttribute>() is null
                   ? Visibility.Visible
                   : Visibility.Collapsed;

        public double Percent { get; set; }

        public StatName StatName { get; }

        public string StatTitle => SharedCharacterStatsHelper.GetFullStatTitle(this.StatName);

        public double Value { get; set; }
    }
}