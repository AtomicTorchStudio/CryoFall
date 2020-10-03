namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Damage;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelItemTooltipWeaponStats : BaseViewModel
    {
        private static readonly Lazy<double> LazyInaccuracyAngleMax
            = new Lazy<double>(CalculateInaccuracyAngleMax);

        private static readonly Lazy<Interval<double>> LazyReloadSpeedRange
            = new Lazy<Interval<double>>(CalculateReloadSpeedRange);

        private static readonly Lazy<double> LazyFireRateMaxRanged
            = new Lazy<double>(() => CalculateFireRateMax(isMelee: false));

        private static readonly Lazy<double> LazyFireRateMaxMelee
            = new Lazy<double>(() => CalculateFireRateMax(isMelee: true));

        private readonly IProtoItemWeapon protoItemWeapon;

        private IItem item;

        public ViewModelItemTooltipWeaponStats(IItem item, IProtoItemWeapon protoItemWeapon)
        {
            this.protoItemWeapon = protoItemWeapon;
            this.Item = item;

            if (protoItemWeapon.FireInterval > 0)
            {
                // this weapon is not a single-shot
                var fireRate = CalculateFireRate(protoItemWeapon);
                var fireRateMax = this.IsRangedWeapon
                                      ? LazyFireRateMaxRanged.Value
                                      : LazyFireRateMaxMelee.Value;

                this.FireRateRating = fireRate / fireRateMax;
                this.HasFireRateRating = true;
            }
            else
            {
                this.HasFireRateRating = false;
            }

            var inaccuracy = CalculateInaccuracyAngle(protoItemWeapon.FirePatternPreset);
            this.AccuracyRating = (LazyInaccuracyAngleMax.Value - inaccuracy)
                                  / LazyInaccuracyAngleMax.Value;

            this.HasReloadSpeed = protoItemWeapon.CompatibleAmmoProtos.Count > 0;

            if (this.HasReloadSpeed)
            {
                var range = LazyReloadSpeedRange.Value;
                this.ReloadSpeed = (CalculateReloadSpeed(protoItemWeapon) - range.Min)
                                   / (range.Max - range.Min);
            }

            this.Refresh();
        }

        public double AccuracyRating { get; }

        public int AmmoCapacity => this.protoItemWeapon.AmmoCapacity;

        public ItemTooltipInfoDamageDescription ControlInfoDamageDescription { get; private set; }

        public IProtoItemAmmo CurrentReferenceAmmoType { get; private set; }

        public double FireRateRating { get; }

        public bool HasFireRateRating { get; }

        public bool HasReloadSpeed { get; }

        public bool IsRangedWeapon => this.protoItemWeapon is IProtoItemWeaponRanged;

        public IItem Item
        {
            get => this.item;
            set
            {
                if (this.item == value)
                {
                    return;
                }

                if (this.item is not null)
                {
                    this.ReleaseSubscriptions();
                }

                this.item = value;

                if (this.item is null)
                {
                    return;
                }

                var weaponPrivateState = this.item.GetPrivateState<WeaponPrivateState>();
                weaponPrivateState.ClientSubscribe(
                    _ => _.CurrentProtoItemAmmo,
                    this.CurrentProtoItemAmmoChanged,
                    this);

                this.Refresh();
            }
        }

        public double ReloadSpeed { get; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.ControlInfoDamageDescription = null;
        }

        private static double CalculateFireRate(IProtoItemWeapon protoWeapon)
        {
            var duration = protoWeapon.FireInterval;
            if (duration == 0
                && protoWeapon.AmmoCapacity > 0
                && protoWeapon.AmmoConsumptionPerShot > 0)
            {
                // actually, this is not requires as we're not displaying the fire rate for single-shot weapons
                // can fire as fast as can reload
                duration = protoWeapon.AmmoReloadDuration
                           * protoWeapon.AmmoConsumptionPerShot
                           / protoWeapon.AmmoCapacity;
            }

            if (duration > 0)
            {
                return 1 / duration;
            }

            return 0;
        }

        private static double CalculateFireRateMax(bool isMelee)
        {
            double maxRate = 0;

            var protoWeapons = isMelee
                                   ? DamageStatsComparisonData.AllAvailableWeaponsMelee.Value
                                   : DamageStatsComparisonData.AllAvailableWeaponsRanged.Value;

            foreach (var protoWeapon in protoWeapons)
            {
                var rate = CalculateFireRate(protoWeapon);
                if (rate > maxRate)
                {
                    maxRate = rate;
                }
            }

            return maxRate;
        }

        private static double CalculateInaccuracyAngle(WeaponFirePatternPreset preset)
        {
            if (!preset.IsEnabled)
            {
                return 0;
            }

            double minAngle = double.MaxValue,
                   maxAngle = double.MinValue;

            foreach (var offset in preset.InitialSequence)
            {
                Process(offset);
            }

            foreach (var offset in preset.CycledSequence)
            {
                Process(offset);
            }

            return maxAngle - minAngle;

            void Process(double offset)
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
        }

        private static double CalculateInaccuracyAngleMax()
        {
            double maxAngle = 0;

            foreach (var protoWeapon in DamageStatsComparisonData.AllAvailableWeaponsRanged.Value)
            {
                var angle = CalculateInaccuracyAngle(protoWeapon.FirePatternPreset);
                if (angle > maxAngle)
                {
                    maxAngle = angle;
                }
            }

            return maxAngle;
        }

        private static double CalculateReloadSpeed(IProtoItemWeapon protoWeapon)
        {
            var duration = protoWeapon.AmmoReloadDuration;
            if (duration > 0)
            {
                return 1 / duration;
            }

            return double.NaN;
        }

        private static Interval<double> CalculateReloadSpeedRange()
        {
            double minSpeed = double.MaxValue,
                   maxSpeed = 0;

            foreach (var protoWeapon in DamageStatsComparisonData.AllAvailableWeaponsRanged.Value)
            {
                var speed = CalculateReloadSpeed(protoWeapon);
                if (double.IsNaN(speed))
                {
                    continue;
                }

                if (speed > maxSpeed)
                {
                    maxSpeed = speed;
                }

                if (speed < minSpeed)
                {
                    minSpeed = speed;
                }
            }

            return (minSpeed, maxSpeed);
        }

        private void CurrentProtoItemAmmoChanged(IProtoItemAmmo currentProtoItemAmmo)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            var protoItemAmmo = this.Item?.GetPrivateState<WeaponPrivateState>()
                                    .CurrentProtoItemAmmo;

            if (protoItemAmmo is null)
            {
                protoItemAmmo = this.protoItemWeapon.ReferenceAmmoProto;
                this.CurrentReferenceAmmoType = protoItemAmmo;
            }
            else
            {
                this.CurrentReferenceAmmoType = null;
            }

            var damageDescription = this.protoItemWeapon.OverrideDamageDescription ?? protoItemAmmo?.DamageDescription;
            if (damageDescription is null)
            {
                this.ControlInfoDamageDescription = null;
                return;
            }

            var fireScatterPreset = protoItemAmmo?.OverrideFireScatterPreset;
            if (!fireScatterPreset.HasValue)
            {
                fireScatterPreset = this.protoItemWeapon.FireScatterPreset;
                if (fireScatterPreset.Value.ProjectileAngleOffets.Length == 1
                    && fireScatterPreset.Value.ProjectileAngleOffets[0] == 0)
                {
                    fireScatterPreset = null;
                }
            }

            var control = ItemTooltipInfoDamageDescription.Create(
                damageDescription,
                fireScatterPreset,
                this.protoItemWeapon.DamageMultiplier,
                this.protoItemWeapon.RangeMultiplier,
                this.protoItemWeapon.DamageStatsComparisonPreset,
                displayRange: true);
            control.Opacity = 1.0;
            control.Margin = default;
            this.ControlInfoDamageDescription = control;
        }
    }
}