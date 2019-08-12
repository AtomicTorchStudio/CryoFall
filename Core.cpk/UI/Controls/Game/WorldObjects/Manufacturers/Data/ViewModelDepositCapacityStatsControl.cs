namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelDepositCapacityStatsControl : BaseViewModel
    {
        public const string TitleDepositDepleted = "This deposit has been depleted.";

        public const string TitleDepositInfinite = "This deposit is infinite.";

        private readonly IProtoObjectDeposit protoDeposit;

        private readonly StaticObjectPublicState publicState;

        private readonly IStaticWorldObject worldObjectDeposit;

        public ViewModelDepositCapacityStatsControl(IStaticWorldObject worldObjectDeposit)
        {
            this.worldObjectDeposit = worldObjectDeposit;
            if (worldObjectDeposit != null)
            {
                this.publicState = worldObjectDeposit.GetPublicState<StaticObjectPublicState>();
                this.protoDeposit = (IProtoObjectDeposit)worldObjectDeposit.ProtoStaticWorldObject;

                this.ValueMax = this.protoDeposit.StructurePointsMax;

                // subscribe on updates
                this.publicState.ClientSubscribe(
                    _ => _.StructurePointsCurrent,
                    _ => this.RefreshDepletion(),
                    this);
            }
            else
            {
                // no deposit (assume depleted)
                this.ValueMax = 0;
            }

            this.RefreshDepletion();

            this.RefreshAvailableToClaim();
        }

        public bool CanClaim => this.CalculateTimeToClaim() <= 0;

        public Visibility DepletedInPrefixVisibility { get; private set; }

        public string DepletionDurationText { get; private set; }

        public string DepositTitle => this.protoDeposit?.Name;

        public string TimeToClaimText
            => ClientTimeFormatHelper.FormatTimeDuration(
                this.CalculateTimeToClaim());

        public float ValueCurrent { get; private set; }

        public float ValueMax { get; private set; }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private static string FormatDuration(double durationSeconds)
        {
            var time = TimeSpan.FromSeconds(durationSeconds);
            return time.TotalDays > 1
                       ? $"{time.TotalDays:F1}" + ClientTimeFormatHelper.SuffixDays
                       : $"{time.TotalHours:F1}" + ClientTimeFormatHelper.SuffixHours;
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private static string FormatInterval(double durationA, double durationB)
        {
            var timeA = TimeSpan.FromSeconds(durationA);
            var timeB = TimeSpan.FromSeconds(durationB);
            return timeA.TotalDays > 1 && timeB.TotalDays > 1
                       ? $"{timeA.TotalDays:F1}~{timeB.TotalDays:F1}" + ClientTimeFormatHelper.SuffixDays
                       : $"{timeA.TotalHours:F1}~{timeB.TotalHours:F1}" + ClientTimeFormatHelper.SuffixHours;
        }

        private int CalculateTimeToClaim()
        {
            if (this.worldObjectDeposit == null)
            {
                return 0;
            }

            var position = WorldMapResourceMarksSystem.SharedGetObjectCenterPosition(this.worldObjectDeposit);
            var mark = WorldMapResourceMarksSystem.SharedEnumerateMarks()
                                                  .FirstOrDefault(m => m.Position == position
                                                                       && m.ProtoWorldObject == this.protoDeposit);

            if (mark.Position == default)
            {
                return 0;
            }

            return (int)WorldMapResourceMarksSystem.SharedCalculateTimeToClaimLimitRemovalSeconds(
                mark.ServerSpawnTime);
        }

        private void RefreshAvailableToClaim()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.CanClaim));
            this.NotifyPropertyChanged(nameof(this.TimeToClaimText));

            if (!this.CanClaim)
            {
                // schedule recursive update in a second
                ClientTimersSystem.AddAction(
                    delaySeconds: 1,
                    this.RefreshAvailableToClaim);
            }
        }

        private void RefreshDepletion()
        {
            this.ValueCurrent = this.publicState?.StructurePointsCurrent ?? 0;
            if (this.ValueCurrent <= 0)
            {
                if (PveSystem.ClientIsPve(logErrorIfDataIsNotYetAvailable: true))
                {
                    // PvE mode doesn't require the deposit under the extractor
                    this.DepletionDurationText = TitleDepositInfinite;
                    this.ValueCurrent = this.ValueMax = 1;
                }
                else
                {
                    this.DepletionDurationText = TitleDepositDepleted;
                }

                this.DepletedInPrefixVisibility = Visibility.Collapsed;
                return;
            }

            var lifetimeTotalDurationSeconds = this.protoDeposit.LifetimeTotalDurationSeconds;
            if (lifetimeTotalDurationSeconds == 0)
            {
                this.DepletionDurationText = TitleDepositInfinite;
                this.DepletedInPrefixVisibility = Visibility.Collapsed;
                return;
            }

            this.DepletedInPrefixVisibility = Visibility.Visible;
            var structurePointsFraction = this.ValueCurrent / this.ValueMax;

            var remainsNoExtraction = structurePointsFraction
                                      * lifetimeTotalDurationSeconds;

            var remainsWithExtraction = remainsNoExtraction
                                        / this.protoDeposit.DecaySpeedMultiplierWhenExtractingActive;

            if (remainsNoExtraction == remainsWithExtraction)
            {
                // display a single duration
                this.DepletionDurationText = FormatDuration(remainsNoExtraction);
            }
            else
            {
                // display an interval
                this.DepletionDurationText = FormatInterval(remainsWithExtraction, remainsNoExtraction);
            }
        }
    }
}