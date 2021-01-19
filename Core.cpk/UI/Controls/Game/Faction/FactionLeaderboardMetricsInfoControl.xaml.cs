namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionLeaderboardMetricsInfoControl : BaseUserControl
    {
        public static readonly DependencyProperty ClanTagProperty
            = DependencyProperty.Register(nameof(ClanTag),
                                          typeof(string),
                                          typeof(FactionLeaderboardMetricsInfoControl),
                                          new PropertyMetadata(null, ClanTagPropertyChanged));

        private Grid layoutRoot;

        private ViewModelFactionLeaderboardMetricsInfoControl viewModel;

        public string ClanTag
        {
            get => (string)this.GetValue(ClanTagProperty);
            set => this.SetValue(ClanTagProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.layoutRoot.DataContext = this.viewModel = new ViewModelFactionLeaderboardMetricsInfoControl();
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private static void ClanTagPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FactionLeaderboardMetricsInfoControl)d).Refresh();
        }

        private async void Refresh()
        {
            if (!this.isLoaded
                || string.IsNullOrEmpty(this.ClanTag))
            {
                return;
            }

            var scoreMetrics = await FactionLeaderboardSystem.ClientGetFactionScoreMetricsAsync(this.ClanTag);
            if (!this.isLoaded)
            {
                return;
            }

            this.viewModel.SetData(scoreMetrics);
        }
    }
}