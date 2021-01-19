namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionRoleTitleEditor : BaseUserControl
    {
        private readonly FactionMemberRole role;

        private FactionOfficerRoleTitle title;

        private ViewModelFactionRoleTitleEditor viewModel;

        public FactionRoleTitleEditor(FactionMemberRole role, FactionOfficerRoleTitle title)
        {
            this.role = role;
            this.title = title;
        }

        public event Action<FactionRoleTitleEditor> SelectedTitleChanged;

        public FactionMemberRole Role => this.role;

        public FactionOfficerRoleTitle SelectedTitle => this.viewModel?.SelectedTitle ?? this.title;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel =
                                   new ViewModelFactionRoleTitleEditor(this.role,
                                                                       this.title,
                                                                       this.ViewModelSelectedTitleChangedHandler);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private void ViewModelSelectedTitleChangedHandler()
        {
            this.title = this.viewModel.SelectedTitle;
            this.SelectedTitleChanged?.Invoke(this);
        }
    }
}