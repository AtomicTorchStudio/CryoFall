namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ShieldProtection.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelShieldProtectionControl : BaseViewModel
    {
        private static readonly Brush StatusTextBrushGreen
            = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0xD0, 0x20));

        private static readonly Brush StatusTextBrushRed
            = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x10, 0x10));

        private static readonly Brush StatusTextBrushYellow
            = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xE0, 0x10));

        private readonly ILogicObject areasGroup;

        private readonly LandClaimAreasGroupPrivateState privateState;

        private readonly LandClaimAreasGroupPublicState publicState;

        private double durationMax;

        private double selectedRechargeTargetFraction = 1.0;

        private ShieldProtectionStatus shieldStatus;

        public ViewModelShieldProtectionControl(ILogicObject areasGroup)
        {
            this.areasGroup = areasGroup;
            this.privateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            this.publicState = LandClaimAreasGroup.GetPublicState(areasGroup);

            this.privateState.ClientSubscribe(
                _ => _.ShieldProtectionCurrentChargeElectricity,
                _ =>
                {
                    this.NotifyPropertyChanged(nameof(this.ElectricityAmount));
                    this.NotifyPropertyChanged(nameof(this.CanActivateShield));
                    this.NotifyPropertyChanged(nameof(this.HasFullCharge));
                    this.UpdateCurrentDurations();
                },
                this);

            this.privateState.ClientSubscribe(
                _ => _.ShieldProtectionCooldownExpirationTime,
                _ => this.RefreshState(),
                this);

            this.publicState.ClientSubscribe(
                _ => _.ShieldActivationTime,
                _ => this.RefreshState(),
                this);

            this.publicState.ClientSubscribe(
                _ => _.Status,
                _ => this.RefreshState(),
                this);

            this.RefreshState();

            this.IsLandClaimInsideAnotherBase = LandClaimShieldProtectionHelper.SharedIsLandClaimInsideAnotherBase(
                areasGroup);

            UpdateCurrentDurationsEverySecond();

            void UpdateCurrentDurationsEverySecond()
            {
                if (this.IsDisposed)
                {
                    return;
                }

                this.UpdateCurrentDurations();
                ClientTimersSystem.AddAction(1, UpdateCurrentDurationsEverySecond);
            }
        }

        public string ActivationDelayText =>
            ClientTimeFormatHelper.FormatTimeDuration(LandClaimShieldProtectionConstants.SharedActivationDuration,
                                                      appendSeconds: false);

        public string ActivationTimeRemainsText { get; private set; }

        public bool CanActivateShield
            => this.shieldStatus == ShieldProtectionStatus.Inactive
               && this.ElectricityAmount > 0;

        public bool CanDeactivateShield
            => this.shieldStatus != ShieldProtectionStatus.Inactive;

        public bool CanRechargeShield
        {
            get
            {
                if (this.shieldStatus == ShieldProtectionStatus.Active)
                {
                    // cannot recharge an active shield
                    return false;
                }

                var currentFraction = this.ElectricityAmount / this.ElectricityCapacity;
                return this.selectedRechargeTargetFraction > currentFraction;
            }
        }

        public BaseCommand CommandActivateShield
            => new ActionCommand(this.ExecuteCommandActivateShield);

        public BaseCommand CommandDeactivateShield
            => new ActionCommand(this.ExecuteCommandDeactivateShield);

        public BaseCommand CommandRechargeShield
            => new ActionCommand(this.ExecuteCommandRechargeShield);

        public string CooldownDurationText =>
            ClientTimeFormatHelper.FormatTimeDuration(LandClaimShieldProtectionConstants.SharedCooldownDuration,
                                                      appendSeconds: false);

        public string CooldownTimeRemainsText { get; private set; }

        public string DurationEstimationText { get; private set; }

        public string DurationMaxText { get; private set; }

        public double ElectricityAmount
            => this.privateState.ShieldProtectionCurrentChargeElectricity;

        public double ElectricityCapacity { get; private set; }

        public bool HasFullCharge => this.ElectricityAmount >= this.ElectricityCapacity;

        public bool IsEnabled => LandClaimShieldProtectionConstants.SharedIsEnabled;

        public bool IsLandClaimInsideAnotherBase { get; }

        public bool IsShieldActive => this.shieldStatus == ShieldProtectionStatus.Active;

        public bool IsShieldProtectionAvailableForCurrentTier
            => this.ElectricityCapacity > 0;

        public string MechanicDescriptionText
            => CoreStrings.ShieldProtection_Description_1
               + "[br][br]"
               + CoreStrings.ShieldProtection_Description_2
               + "[br]"
               + CoreStrings.ShieldProtection_Description_3
               + "[br]"
               + CoreStrings.ShieldProtection_Description_7
               + "[br]"
               + CoreStrings.ShieldProtection_Description_4
               + "[br]"
               + CoreStrings.ShieldProtection_Description_8;

        public string RequiresUpgradeText
            => CoreStrings.ShieldProtection_UpgradeToUnlock
               + "[br][br]"
               + CoreStrings.ShieldProtection_Description_1;

        public string SelectedRechargeTargetDurationText
        {
            get
            {
                if (this.ElectricityCapacity <= 0)
                {
                    return null;
                }

                return ClientTimeFormatHelper.FormatTimeDuration(
                    this.EstimateDuration(this.GetTargetChargeElectricity()),
                    appendSeconds: false);
            }
        }

        public string SelectedRechargeTargetElectricityAmountText
            => this.GetTargetChargeElectricity()
                   .ToString("F0")
               + " "
               + CoreStrings.EnergyUnitAbbreviation;

        public double SelectedRechargeTargetFraction
        {
            get => this.selectedRechargeTargetFraction;
            set
            {
                if (value == this.selectedRechargeTargetFraction)
                {
                    return;
                }

                this.selectedRechargeTargetFraction = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.SelectedRechargeTargetElectricityAmountText));
                this.NotifyPropertyChanged(nameof(this.SelectedRechargeTargetDurationText));
                this.NotifyPropertyChanged(nameof(this.CanRechargeShield));

                this.SelectedRechargeTargetFraction =
                    this.NormalizeSelectedRechargeTargetFraction(this.selectedRechargeTargetFraction);
            }
        }

        public string StatusText { get; private set; }

        public Brush StatusTextBrush { get; private set; }

        private static Brush GetStatusTextBrush(ShieldProtectionStatus status)
            => status switch
            {
                ShieldProtectionStatus.Inactive   => StatusTextBrushRed,
                ShieldProtectionStatus.Activating => StatusTextBrushYellow,
                ShieldProtectionStatus.Active     => StatusTextBrushGreen,
                _                                 => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };

        private double EstimateDuration(double charge)
        {
            return LandClaimShieldProtectionSystem.SharedCalculateShieldEstimatedDuration(charge,
                                                                                          this.durationMax,
                                                                                          this.ElectricityCapacity);
        }

        private void ExecuteCommandActivateShield()
        {
            LandClaimShieldProtectionSystem.ClientActivateShield(this.areasGroup);
        }

        private void ExecuteCommandDeactivateShield()
        {
            LandClaimShieldProtectionSystem.ClientDeactivateShield(this.areasGroup);
        }

        private void ExecuteCommandRechargeShield()
        {
            LandClaimShieldProtectionSystem.ClientRechargeShield(
                this.areasGroup,
                targetChargeElectricity: this.GetTargetChargeElectricity());
        }

        private double GetTargetChargeElectricity()
        {
            return Math.Round(this.SelectedRechargeTargetFraction * this.ElectricityCapacity,
                              MidpointRounding.AwayFromZero);
        }

        private double NormalizeSelectedRechargeTargetFraction(double fraction)
        {
            var currentFraction = this.ElectricityAmount / this.ElectricityCapacity;
            return Math.Max(currentFraction, fraction);
        }

        private void RefreshState()
        {
            this.shieldStatus = LandClaimShieldProtectionSystem.SharedGetShieldPublicStatus(this.areasGroup);
            this.StatusText = this.shieldStatus.GetDescription();
            this.StatusTextBrush = GetStatusTextBrush(this.shieldStatus);

            LandClaimShieldProtectionSystem.SharedGetShieldProtectionMaxStatsForBase(
                this.areasGroup,
                out var maxShieldDuration,
                out var shieldProtectionElectricityCapacity);

            this.durationMax = maxShieldDuration;
            this.DurationMaxText = ClientTimeFormatHelper.FormatTimeDuration(maxShieldDuration, appendSeconds: false);

            this.ElectricityCapacity = shieldProtectionElectricityCapacity;

            this.NotifyPropertyChanged(nameof(this.IsShieldActive));
            this.NotifyPropertyChanged(nameof(this.CanActivateShield));
            this.NotifyPropertyChanged(nameof(this.CanDeactivateShield));
            this.NotifyPropertyChanged(nameof(this.ElectricityAmount));
            this.NotifyPropertyChanged(nameof(this.SelectedRechargeTargetElectricityAmountText));
            this.NotifyPropertyChanged(nameof(this.SelectedRechargeTargetDurationText));
            this.NotifyPropertyChanged(nameof(this.CanRechargeShield));
            this.NotifyPropertyChanged(nameof(this.HasFullCharge));
            this.NotifyPropertyChanged(nameof(this.IsShieldProtectionAvailableForCurrentTier));

            this.UpdateCurrentDurations();
        }

        private void UpdateCurrentDurations()
        {
            if (this.ElectricityCapacity > 0)
            {
                var durationEstimation =
                    this.EstimateDuration(this.privateState.ShieldProtectionCurrentChargeElectricity);
                this.DurationEstimationText = durationEstimation <= 0
                                                  ? "0"
                                                  : ClientTimeFormatHelper.FormatTimeDuration(
                                                      durationEstimation,
                                                      appendSeconds: false);
            }
            else
            {
                this.DurationEstimationText = null;
            }

            if (this.shieldStatus == ShieldProtectionStatus.Activating)
            {
                var activatingTimeRemains = this.publicState.ShieldActivationTime
                                            - Client.CurrentGame.ServerFrameTimeApproximated;
                activatingTimeRemains = Math.Max(0, activatingTimeRemains);

                this.ActivationTimeRemainsText = string.Format(CoreStrings.TextInParenthesisFormat,
                                                               ClientTimeFormatHelper.FormatTimeDuration(
                                                                   activatingTimeRemains,
                                                                   appendSeconds: true));
            }
            else
            {
                this.ActivationTimeRemainsText = null;
            }

            var remainingCooldown = LandClaimShieldProtectionSystem.SharedCalculateCooldownRemains(this.areasGroup);
            if (this.shieldStatus == ShieldProtectionStatus.Inactive
                && remainingCooldown > 0)
            {
                this.CooldownTimeRemainsText
                    = string.Format(CoreStrings.ShieldProtection_CooldownRemains_Format,
                                    ClientTimeFormatHelper.FormatTimeDuration(
                                        remainingCooldown,
                                        appendSeconds: true));
            }
            else
            {
                this.CooldownTimeRemainsText = null;
            }
        }
    }
}