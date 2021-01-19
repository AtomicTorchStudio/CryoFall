namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionDiplomacyListControl : BaseUserControl
    {
        public static readonly DependencyProperty DiplomacyStatusFilterProperty
            = DependencyProperty.Register("DiplomacyStatusFilter",
                                          typeof(FactionDiplomacyStatus),
                                          typeof(FactionDiplomacyListControl),
                                          new PropertyMetadata(default(FactionDiplomacyStatus)));

        private ViewModelFactionDiplomacyListControl viewModel;

        public FactionDiplomacyStatus DiplomacyStatusFilter
        {
            get => (FactionDiplomacyStatus)this.GetValue(DiplomacyStatusFilterProperty);
            set => this.SetValue(DiplomacyStatusFilterProperty, value);
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelFactionDiplomacyListControl(this.DiplomacyStatusFilter);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}