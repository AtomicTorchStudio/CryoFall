namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Damage;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipInfoDamageDescription : BaseUserControl
    {
        private static readonly Lazy<double> LazyMaxFireSpread
            = new Lazy<double>(CalculateFireSpreadMax);

        private DamageStatsComparisonPreset comparisonPreset;

        private DamageDescription damageDescription;

        private double damageMultiplier;

        private bool displayRange;

        private WeaponFireScatterPreset? fireScatterPreset;

        private double rangeMultiplier;

        private ViewModelItemTooltipInfoDamageDescription viewModel;

        public static ItemTooltipInfoDamageDescription Create(
            DamageDescription damageDescription,
            WeaponFireScatterPreset? fireScatterPreset,
            double damageMultiplier,
            double rangeMultiplier,
            DamageStatsComparisonPreset comparisonPreset,
            bool displayRange)
        {
            return new ItemTooltipInfoDamageDescription()
            {
                damageDescription = damageDescription,
                fireScatterPreset = fireScatterPreset,
                damageMultiplier = damageMultiplier,
                rangeMultiplier = rangeMultiplier,
                displayRange = displayRange,
                comparisonPreset = comparisonPreset
            };
        }

        protected override void OnLoaded()
        {
            var max = DamageStatsComparisonHelper.MaxPresets[this.comparisonPreset];

            var stoppingPower = this.damageDescription.FinalDamageMultiplier;
            var stoppingPowerMax = DamageStatsComparisonHelper.MaxPresets.Max(p => p.Value.FinalDamageMultiplier);

            stoppingPower -= 1;
            stoppingPowerMax -= 1;

            stoppingPower = stoppingPowerMax
                            * Math.Sqrt(stoppingPower / stoppingPowerMax);

            var isRangedWeapon = this.comparisonPreset.IsRangedWeapon;

            this.viewModel = new ViewModelItemTooltipInfoDamageDescription(
                damage: this.damageDescription.DamageValue * this.damageMultiplier,
                damageMax: max.DamageValue,
                armorPiercing: this.damageDescription.ArmorPiercingCoef,
                armorPiercingMax: 1,
                displayStoppingPower: isRangedWeapon,
                stoppingPower: stoppingPower,
                stoppingPowerMax: stoppingPowerMax,
                displayRange: isRangedWeapon && this.displayRange,
                range: this.damageDescription.RangeMax * this.rangeMultiplier,
                rangeMax: max.RangeMax,
                hasSpread: isRangedWeapon && this.fireScatterPreset.HasValue,
                spread: isRangedWeapon ? CalculateFireSpread(this.fireScatterPreset) : 0,
                spreadMax: isRangedWeapon ? LazyMaxFireSpread.Value : 0,
                damageProportions: this.damageDescription.DamageProportions);

            this.DataContext = this.viewModel;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private static double CalculateFireSpread(WeaponFireScatterPreset? preset)
        {
            if (!preset.HasValue)
            {
                return 0;
            }

            var offsets = preset.Value.ProjectileAngleOffets;
            double minAngle = double.MaxValue,
                   maxAngle = double.MinValue;

            foreach (var offset in offsets)
            {
                if (offset < minAngle)
                {
                    minAngle = offset;
                }

                if (offset > maxAngle)
                {
                    maxAngle = offset;
                }
            }

            return maxAngle - minAngle;
        }

        private static double CalculateFireSpreadMax()
        {
            double maxSpread = 0;

            foreach (var protoAmmo in DamageStatsComparisonData.AllAvailableAmmoExceptGrenades.Value)
            {
                var d = protoAmmo.DamageDescription;
                if (d.DamageValue == 0)
                {
                    continue;
                }

                var spread = CalculateFireSpread(protoAmmo.OverrideFireScatterPreset);
                if (spread > maxSpread)
                {
                    maxSpread = spread;
                }
            }

            foreach (var protoWeapon in DamageStatsComparisonData.AllAvailableWeaponsRanged.Value)
            {
                var spread = CalculateFireSpread(protoWeapon.FireScatterPreset);
                if (spread > maxSpread)
                {
                    maxSpread = spread;
                }
            }

            return maxSpread;
        }
    }
}