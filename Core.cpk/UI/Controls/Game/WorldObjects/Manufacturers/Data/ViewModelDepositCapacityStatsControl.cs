namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelDepositCapacityStatsControl : BaseViewModel
    {
        private readonly IProtoObjectDeposit protoDeposit;

        private readonly StaticObjectPublicState publicState;

        private readonly Vector2Ushort tilePosition;

        private readonly IStaticWorldObject worldObjectDeposit;

        public ViewModelDepositCapacityStatsControl(IStaticWorldObject worldObjectDeposit, Vector2Ushort tilePosition)
        {
            this.worldObjectDeposit = worldObjectDeposit;
            this.tilePosition = tilePosition;
            if (worldObjectDeposit is not null)
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

            this.RefreshAvailableToClaim();

            this.RefreshByTimer();
        }

        public bool CanClaim => this.CalculateTimeToClaim() <= 0;

        public Visibility DepletedDurationVisibility { get; private set; }

        public string DepletionDurationText { get; private set; }

        public string DepositTitle => this.protoDeposit?.Name;

        public bool IsDepletedDeposit { get; set; }

        public bool IsDepositCapacityVisible
            => !this.IsInfiniteDeposit
               || !PveSystem.ClientIsPve(false);

        public bool IsInfiniteDeposit { get; set; }

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
            if (this.worldObjectDeposit is null)
            {
                return 0;
            }

            var mark = WorldMapResourceMarksSystem.SharedEnumerateMarks()
                                                  .FirstOrDefault(m => m.Id == this.worldObjectDeposit.Id);

            return (int)WorldMapResourceMarksSystem.SharedCalculateTimeToClaimLimitRemovalSeconds(mark.ServerSpawnTime);
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

        private void RefreshByTimer()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.RefreshDepletion();
            ClientTimersSystem.AddAction(delaySeconds: 1,
                                         this.RefreshByTimer);
        }

        private void RefreshDepletion()
        {
            var depletedDeposit = ObjectDepletedDeposit.SharedGetDepletedDepositWorldObject(this.tilePosition);
            if (depletedDeposit is not null)
            {
                // depleted deposit
                this.IsInfiniteDeposit = false;
                this.IsDepletedDeposit = true;
                this.DepletionDurationText = null;
                this.DepletedDurationVisibility = Visibility.Collapsed;
                this.ValueMax = 1;
                this.ValueCurrent = 0;
                return;
            }

            this.IsDepletedDeposit = false;

            var currentValue = this.publicState is not null
                               && this.protoDeposit is not null
                               && this.protoDeposit.LifetimeTotalDurationSeconds > 0
                                   ? this.publicState.StructurePointsCurrent
                                   : 0;
            if (currentValue <= 0)
            {
                // no deposit/infinite deposit
                this.IsInfiniteDeposit = true;
                this.DepletionDurationText = CoreStrings.DepositCapacityStats_TitleInfinite;
                this.DepletedDurationVisibility = Visibility.Collapsed;
                this.ValueCurrent = this.ValueMax = 1;
                return;
            }

            var lifetimeTotalDurationSeconds = this.protoDeposit.LifetimeTotalDurationSeconds;
            this.ValueCurrent = currentValue;

            this.IsInfiniteDeposit = false;
            this.DepletedDurationVisibility = Visibility.Visible;
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