namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionDiplomacyPublicInfoControl : BaseUserControl
    {
        public static readonly DependencyProperty ClanTagProperty
            = DependencyProperty.Register(nameof(ClanTag),
                                          typeof(string),
                                          typeof(FactionDiplomacyPublicInfoControl),
                                          new PropertyMetadata(null, ClanTagPropertyChanged));

        private Grid layoutRoot;

        private ViewModelFactionDiplomacyPublicInfoControl viewModel;

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
            this.layoutRoot.DataContext = this.viewModel = new ViewModelFactionDiplomacyPublicInfoControl();
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
            ((FactionDiplomacyPublicInfoControl)d).Refresh();
        }

        private async void Refresh()
        {
            if (!this.isLoaded
                || string.IsNullOrEmpty(this.ClanTag))
            {
                return;
            }

            var relations = await FactionSystem.ClientFactionDiplomacyPublicInfo(this.ClanTag);
            if (!this.isLoaded)
            {
                return;
            }

            this.viewModel.SetData(relations);
        }
    }
}