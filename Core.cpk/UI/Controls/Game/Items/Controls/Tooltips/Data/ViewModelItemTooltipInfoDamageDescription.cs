namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelItemTooltipInfoDamageDescription : BaseViewModel
    {
        public ViewModelItemTooltipInfoDamageDescription(
            double damage,
            double damageMax,
            double armorPiercing,
            double armorPiercingMax,
            bool displayStoppingPower,
            double stoppingPower,
            double stoppingPowerMax,
            bool displayRange,
            double range,
            double rangeMax,
            bool hasSpread,
            double spread,
            double spreadMax,
            IReadOnlyList<DamageProportion> damageProportions)
        {
            this.Damage = damage;
            this.DamageMax = damageMax;
            this.ArmorPiercing = armorPiercing;
            this.ArmorPiercingMax = armorPiercingMax;
            this.DisplayStoppingPower = displayStoppingPower;
            this.StoppingPower = stoppingPower;
            this.StoppingPowerMax = stoppingPowerMax;
            this.DisplayRange = displayRange;
            this.Range = range;
            this.RangeMax = rangeMax;
            this.HasSpread = hasSpread;
            this.Spread = spread;
            this.SpreadMax = spreadMax;

            var list = new List<DataDamageProportion>();
            foreach (var entry in damageProportions)
            {
                list.Add(new DataDamageProportion(entry.DamageType,
                                                  (byte)(100 * entry.Proportion)));
            }

            list.SortByDesc(d => d.Proportion);

            this.DamageProportions = list;
        }

        public double ArmorPiercing { get; }

        public double ArmorPiercingMax { get; }

        public double Damage { get; }

        public double DamageMax { get; }

        public IReadOnlyList<DataDamageProportion> DamageProportions { get; }

        public bool DisplayRange { get; }

        public bool DisplayStoppingPower { get; }

        public bool HasSpread { get; }

        public double Range { get; }

        public double RangeMax { get; }

        public double Spread { get; }

        public double SpreadMax { get; }

        public double StoppingPower { get; }

        public double StoppingPowerMax { get; }

        public readonly struct DataDamageProportion
        {
            public DataDamageProportion(DamageType damageType, byte proportion)
            {
                this.DamageType = damageType;
                this.Proportion = proportion;
            }

            public DamageType DamageType { get; }

            public string DamageTypeName => this.DamageType.GetDescription();

            public ImageSource IconImageSource => ClientDamageTypeIconHelper.GetImageSource(this.DamageType);

            public byte Proportion { get; }
        }
    }
}