namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System;
    using System.Globalization;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.RaidingProtection;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;

    public class ViewModelOfflineRaidingProtectionControl : BaseViewModel
    {
        private static readonly string ShortTimePattern
            = CultureInfo.InstalledUICulture.DateTimeFormat.ShortTimePattern;

        public ViewModelOfflineRaidingProtectionControl()
        {
            RaidingProtectionSystem.ClientRaidingWindowChanged += this.RefreshAll;
            PveSystem.ClientIsPvEChanged += this.RefreshAll;

            this.UpdateNextRaidingInfo();
        }

        public string CurrentOrNextRaidingTimeInfo { get; private set; }

        public bool IsRaidingProtectionEnabled
            => RaidingProtectionSystem.ClientIsRaidingWindowEnabled;

        public string RaidingProtectionDescription
        {
            get
            {
                if (!RaidingProtectionSystem.ClientIsRaidingWindowEnabled)
                {
                    return CoreStrings.WindowPolitics_RaidingRestriction_Disabled;
                }

                var timeInterval = RaidingProtectionSystem.ClientRaidingWindowUTC;

                var todayUtc = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
                var fromDate = todayUtc + TimeSpan.FromHours(timeInterval.FromHour);
                var toDate = todayUtc + TimeSpan.FromHours(timeInterval.ToHourNormalized);

                fromDate = TimeZone.CurrentTimeZone.ToLocalTime(fromDate);
                toDate = TimeZone.CurrentTimeZone.ToLocalTime(toDate);

                var inTotal = timeInterval.DurationHours.ToString("0.##") + ClientTimeFormatHelper.SuffixHours;

                return string.Format(
                    CoreStrings.WindowPolitics_RaidingRestriction_DescriptionFormat,
                    fromDate.ToString(ShortTimePattern).Replace(" ", "\u00A0"),
                    toDate.ToString(ShortTimePattern).Replace(" ", "\u00A0"),
                    inTotal);
            }
        }

        public Visibility Visibility
            => PveSystem.ClientIsPve(logErrorIfDataIsNotYetAvailable: false)
                   ? Visibility.Collapsed
                   : Visibility.Visible;

        public void UpdateNextRaidingInfo()
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (Menu.IsOpened<WindowPolitics>())
            {
                this.CurrentOrNextRaidingTimeInfo = GetNextRaidingTime();
            }

            // schedule next update
            ClientTimersSystem.AddAction(
                delaySeconds: 1,
                this.UpdateNextRaidingInfo);
        }

        protected override void DisposeViewModel()
        {
            RaidingProtectionSystem.ClientRaidingWindowChanged -= this.RefreshAll;
            PveSystem.ClientIsPvEChanged -= this.RefreshAll;
        }

        private static string GetNextRaidingTime()
        {
            if (!RaidingProtectionSystem.ClientIsRaidingWindowEnabled)
            {
                return null;
            }

            var timeUntilNextRaid = RaidingProtectionSystem.SharedCalculateTimeUntilNextRaid();
            if (timeUntilNextRaid.TotalHours <= 0)
            {
                // raiding is going on
                var timeUntilRaidEnds = RaidingProtectionSystem.SharedCalculateTimeUntilRaidEnds();

                return string.Format(
                    CoreStrings.WindowPolitics_RaidingRestriction_CurrentRaidWindow,
                    ClientTimeFormatHelper.FormatTimeDuration(timeUntilRaidEnds, trimRemainder: false));
            }

            return string.Format(
                CoreStrings.WindowPolitics_RaidingRestriction_NextRaidWindowTimeout,
                ClientTimeFormatHelper.FormatTimeDuration(timeUntilNextRaid, trimRemainder: false));
        }

        private void RefreshAll()
        {
            this.NotifyPropertyChanged(nameof(this.IsRaidingProtectionEnabled));
            this.NotifyPropertyChanged(nameof(this.CurrentOrNextRaidingTimeInfo));
            this.NotifyPropertyChanged(nameof(this.RaidingProtectionDescription));
            this.NotifyPropertyChanged(nameof(this.Visibility));
        }
    }
}