namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Plants.Data
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelFarmPlantTooltip : BaseViewModel
    {
        public const double TimerRefreshIntervalSeconds = 1 / 60.0;

        public const string TitlePermanent = "permanent";

        private readonly IStaticWorldObject objectPlant;

        private readonly IProtoObjectPlant protoPlant;

        private readonly PlantPublicState publicState;

        private IProtoItemFertilizer appliedFertilizedProto;

        private bool isDataReceivedFromServer;

        private double nextHarvestOrSpoilTime;

        private double totalHarvestDuration;

        private double wateringDuration;

        private double wateringEndsTime;

        public ViewModelFarmPlantTooltip(IStaticWorldObject objectPlant, PlantPublicState publicState)
        {
            this.objectPlant = objectPlant;
            this.protoPlant = (IProtoObjectPlant)objectPlant.ProtoStaticWorldObject;
            this.publicState = publicState;

            this.HarvestsCountMax = this.protoPlant.NumberOfHarvests;

            publicState.ClientSubscribe(
                _ => _.HasHarvest,
                _ =>
                {
                    this.NotifyPropertyChanged(nameof(this.HasHarvest));
                    this.RefreshDataFromServer();
                },
                this);

            publicState.ClientSubscribe(
                _ => _.IsFertilized,
                _ => this.RefreshDataFromServer(),
                this);

            publicState.ClientSubscribe(
                _ => _.IsSpoiled,
                _ => this.NotifyPropertyChanged(nameof(this.IsSpoiled)),
                this);

            publicState.ClientSubscribe(
                _ => _.IsWatered,
                isWatered =>
                {
                    if (isWatered)
                    {
                        this.wateringEndsTime = double.MaxValue;
                        this.UpdateDisplayedTimeNoTimer();
                    }

                    this.UpdateWatered();
                    this.RefreshDataFromServer();
                },
                this);

            this.RefreshDataFromServer();
            this.UpdateWatered();
        }

        public ViewModelFarmPlantTooltip()
        {
        }

        public string FertilizerBonusText { get; private set; }

        public float HarvestInTimePercent { get; private set; } = 50;

        public string HarvestInTimeText { get; private set; } = "...";

        public byte HarvestsCount { get; private set; }

        public byte HarvestsCountMax { get; }

        public bool HasHarvest => this.publicState.HasHarvest;

        public Brush IconFertilizer =>
            this.appliedFertilizedProto != null
                ? Api.Client.UI.GetTextureBrush(this.appliedFertilizedProto.Icon)
                : null;

        public Brush IconPlant => Api.Client.UI.GetTextureBrush(this.protoPlant.IconFullGrown);

        public Brush IconSkillFarming
            => Api.Client.UI.GetTextureBrush(Api.GetProtoEntity<SkillFarming>().Icon);

        public bool IsSpoiled => this.publicState.IsSpoiled;

        public bool IsSpoiling { get; private set; }

        public string SkillGrowthSpeedBonusText { get; private set; }

        public float SpoiledInTimePercent { get; private set; } = 50;

        public string SpoiledInTimeText { get; private set; } = "...";

        public string Title => this.protoPlant.Name;

        public Visibility Visibility { get; set; } = IsDesignTime ? Visibility.Visible : Visibility.Hidden;

        public Visibility VisibilityDataNotReceived { get; set; } = Visibility.Visible;

        public Visibility VisibilityDataReceived { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilityNotWatered { get; private set; }

        public Visibility VisibilityWatered { get; private set; }

        public float WateringEndsTimePercent { get; private set; } = 50;

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        public string WateringEndsTimeText { get; private set; } = "23h 59m 59s";

        public string WateringSpeedBonusText { get; private set; }

        private async void RefreshDataFromServer()
        {
            var data = await this.protoPlant.ClientGetTooltipData(this.objectPlant);
            if (this.IsDisposed)
            {
                return;
            }

            this.Visibility = Visibility.Visible;

            if (this.appliedFertilizedProto != data.AppliedFertilzerProto)
            {
                this.appliedFertilizedProto = data.AppliedFertilzerProto;
                this.FertilizerBonusText = this.appliedFertilizedProto?.FertilizerShortDescription;
                this.NotifyPropertyChanged(nameof(this.IconFertilizer));
            }

            this.HarvestsCount = data.ProducedHarvestsCount;
            this.nextHarvestOrSpoilTime = data.ServerTimeNextHarvestOrSpoil;
            this.totalHarvestDuration = this.protoPlant.ClientCalculateHarvestTotalDuration(
                                            onlyForHarvestStage: this.HarvestsCount > 0)
                                        / data.SpeedMultiplier;

            this.wateringEndsTime = data.ServerTimeWateringEnds;
            this.wateringDuration = data.LastWateringDuration;

            this.SkillGrowthSpeedBonusText =
                data.SkillGrowthSpeedMultiplier > 1.0
                    ? string.Format(ItemMulch.ShortDescriptionText,
                                    "+"
                                    + (int)Math.Ceiling(100 * (data.SkillGrowthSpeedMultiplier - 1.0)))
                    : null;

            if (!this.isDataReceivedFromServer)
            {
                this.isDataReceivedFromServer = true;
                this.VisibilityDataReceived = Visibility.Visible;
                this.VisibilityDataNotReceived = Visibility.Collapsed;

                // start updating displayed time
                this.TimerUpdateDisplayedTime();
            }
        }

        private void TimerUpdateDisplayedTime()
        {
            if (this.IsDisposed)
            {
                // view model is disposed - stop updating
                return;
            }

            this.UpdateDisplayedTimeNoTimer();

            // schedule refresh of the displayed time
            ClientTimersSystem.AddAction(
                TimerRefreshIntervalSeconds,
                this.TimerUpdateDisplayedTime);
        }

        private void UpdateDisplayedTimeNoTimer()
        {
            if (this.publicState.HasHarvest)
            {
                this.HarvestInTimePercent = 100;
                this.HarvestInTimeText = null; // not used
            }
            else
            {
                // update harvest time
                var fraction = ServerTimersSystem.SharedGetTimeRemainingFraction(
                    this.nextHarvestOrSpoilTime,
                    this.totalHarvestDuration,
                    out var timeRemainingSeconds);

                this.HarvestInTimeText = ClientTimeFormatHelper.FormatTimeDuration(Math.Max(0, timeRemainingSeconds));
                this.HarvestInTimePercent = (float)(100 * fraction);
            }

            if (this.publicState.IsSpoiled)
            {
                this.SpoiledInTimePercent = 100;
                this.SpoiledInTimeText = null; // not used
                this.IsSpoiling = true;
            }
            else if (this.publicState.HasHarvest)
            {
                // update rotten time
                var fraction = ServerTimersSystem.SharedGetTimeRemainingFraction(
                    this.nextHarvestOrSpoilTime,
                    this.protoPlant.TimeToHarvestSpoilTotalSeconds / FarmingConstants.SharedFarmPlantsSpoilSpeedMultiplier,
                    out var timeRemainingSeconds);

                this.SpoiledInTimeText = ClientTimeFormatHelper.FormatTimeDuration(Math.Max(0, timeRemainingSeconds));
                this.SpoiledInTimePercent = (float)(100 * fraction);
                this.IsSpoiling = true;
            }
            else
            {
                this.IsSpoiling = false;
            }

            if (this.VisibilityWatered == Visibility.Visible)
            {
                // update watering time
                var totalDuration = this.wateringDuration;
                if (totalDuration < double.MaxValue)
                {
                    var fraction = ServerTimersSystem.SharedGetTimeRemainingFraction(
                        this.wateringEndsTime,
                        totalDuration,
                        out var timeRemainingSeconds);
                    this.WateringEndsTimeText = ClientTimeFormatHelper.FormatTimeDuration(timeRemainingSeconds);
                    this.WateringEndsTimePercent = (float)(100 * fraction);
                }
                else
                {
                    this.WateringEndsTimeText = TitlePermanent;
                    this.WateringEndsTimePercent = 100;
                }
            }
            else
            {
                this.WateringEndsTimePercent = 0;
            }
        }

        private void UpdateWatered()
        {
            var isWatered = this.publicState.IsWatered;
            this.VisibilityWatered = isWatered ? Visibility.Visible : Visibility.Collapsed;
            this.VisibilityNotWatered = !isWatered ? Visibility.Visible : Visibility.Collapsed;

            this.WateringSpeedBonusText =
                isWatered
                    ? string.Format(ItemMulch.ShortDescriptionText,
                                    "+"
                                    + (int)Math.Ceiling(100 * (FarmingConstants.WateringGrowthSpeedMultiplier - 1.0)))
                    : null;
        }
    }
}