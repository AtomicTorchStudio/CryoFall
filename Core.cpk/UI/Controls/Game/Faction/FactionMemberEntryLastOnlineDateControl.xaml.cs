namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionMemberEntryLastOnlineDateControl : BaseUserControl
    {
        public static readonly DependencyProperty MemberNameProperty =
            DependencyProperty.Register("MemberName",
                                        typeof(string),
                                        typeof(FactionMemberEntryLastOnlineDateControl),
                                        new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(nameof(Text),
                                          typeof(string),
                                          typeof(FactionMemberEntryLastOnlineDateControl),
                                          new PropertyMetadata(default(string)));

        public string MemberName
        {
            get => (string)this.GetValue(MemberNameProperty);
            set => this.SetValue(MemberNameProperty, value);
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public static string FormatDateDate(double offlineDuration)
        {
            var deltaTime = TimeSpan.FromSeconds(offlineDuration);

            var totalDays = deltaTime.TotalDays;
            if (totalDays > 10000)
            {
                return "—";
            }

            if (totalDays >= 2)
            {
                return string.Format(CoreStrings.WipedDate_DaysAgo_Format, (int)totalDays);
            }

            var totalHours = deltaTime.TotalHours;
            if (totalHours >= 1)
            {
                return string.Format(CoreStrings.WipedDate_HoursAgo_Format, (int)totalHours);
            }

            return CoreStrings.RelativeDate_LessThanHourAgo;
        }

        protected override void InitControl()
        {
            this.Text = string.Format(CoreStrings.Faction_Member_LastOnlineDate_Format,
                                      "...");
        }

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        private async void Refresh()
        {
            var lastOnlineDate = await FactionSystem.ClientGetLastOnlineDate(this.MemberName);
            if (!this.isLoaded)
            {
                return;
            }

            var timeSinceLastOnline = Api.Client.CurrentGame.ServerFrameTimeApproximated - lastOnlineDate;
            if (timeSinceLastOnline < 0)
            {
                timeSinceLastOnline = 0;
            }

            this.Text = string.Format(CoreStrings.Faction_Member_LastOnlineDate_Format,
                                      FormatDateDate(timeSinceLastOnline));
        }
    }
}