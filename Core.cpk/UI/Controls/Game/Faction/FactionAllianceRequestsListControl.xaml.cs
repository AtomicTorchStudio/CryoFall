namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionAllianceRequestsListControl : BaseUserControl
    {
        public static readonly DependencyProperty IsIncomingRequestsProperty
            = DependencyProperty.Register(nameof(IsIncomingRequests),
                                          typeof(bool),
                                          typeof(FactionAllianceRequestsListControl),
                                          new PropertyMetadata(default(bool)));

        private ViewModelFactionAllianceRequestsListControl viewModel;

        public bool IsIncomingRequests
        {
            get => (bool)this.GetValue(IsIncomingRequestsProperty);
            set => this.SetValue(IsIncomingRequestsProperty, value);
        }

        protected override void OnLoaded()
        {
            this.DataContext =
                this.viewModel = new ViewModelFactionAllianceRequestsListControl(this.IsIncomingRequests);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}