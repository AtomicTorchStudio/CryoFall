namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionMembersListControl : BaseUserControl
    {
        public static readonly DependencyProperty ColumnsCountProperty =
            DependencyProperty.Register(nameof(ColumnsCount),
                                        typeof(int),
                                        typeof(FactionMembersListControl),
                                        new PropertyMetadata(defaultValue: 3));

        public static readonly DependencyProperty SortByRoleProperty =
            DependencyProperty.Register("SortByRole",
                                        typeof(bool),
                                        typeof(FactionMembersListControl),
                                        new PropertyMetadata(default(bool)));

        private ViewModelFactionMembersListControl viewModel;

        public int ColumnsCount
        {
            get => (int)this.GetValue(ColumnsCountProperty);
            set => this.SetValue(ColumnsCountProperty, value);
        }

        public bool SortByRole
        {
            get => (bool)this.GetValue(SortByRoleProperty);
            set => this.SetValue(SortByRoleProperty, value);
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelFactionMembersListControl(this.SortByRole);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}