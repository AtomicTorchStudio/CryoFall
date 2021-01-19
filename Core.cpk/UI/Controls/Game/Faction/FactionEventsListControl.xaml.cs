namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionEventsListControl : BaseUserControl
    {
        public static readonly DependencyProperty IsRecentEventsListProperty
            = DependencyProperty.Register(nameof(IsRecentEventsList),
                                          typeof(bool),
                                          typeof(FactionEventsListControl),
                                          new PropertyMetadata(default(bool)));

        private ViewModelFactionEventsListControl viewModel;

        public bool IsRecentEventsList
        {
            get => (bool)this.GetValue(IsRecentEventsListProperty);
            set => this.SetValue(IsRecentEventsListProperty, value);
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelFactionEventsListControl();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}