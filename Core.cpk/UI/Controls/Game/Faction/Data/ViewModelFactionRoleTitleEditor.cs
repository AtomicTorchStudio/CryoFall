namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelFactionRoleTitleEditor : BaseViewModel
    {
        private readonly Action selectedTitleChanged;

        private FactionOfficerRoleTitle selectedTitle;

        public ViewModelFactionRoleTitleEditor(
            FactionMemberRole factionMemberRole,
            FactionOfficerRoleTitle selectedTitle,
            Action selectedTitleChanged)
        {
            this.AllTitles = EnumHelper.EnumValuesToViewModel<FactionOfficerRoleTitle>()
                                       .OrderBy(e => e.Description, StringComparer.OrdinalIgnoreCase)
                                       .ToArray();

            this.FactionMemberRole = factionMemberRole;
            this.selectedTitle = selectedTitle;
            this.selectedTitleChanged = selectedTitleChanged;
        }

        public ViewModelEnum<FactionOfficerRoleTitle>[] AllTitles { get; }

        public BaseCommand CommandDisplayCombobox
            => new ActionCommand(() => this.IsComboboxVisible = true);

        public BaseCommand CommandHideCombobox
            => new ActionCommand(() => this.IsComboboxVisible = false);

        public FactionMemberRole FactionMemberRole { get; }

        public bool IsComboboxVisible { get; set; }

        public FactionOfficerRoleTitle SelectedTitle
        {
            get => this.selectedTitle;
            set
            {
                if (this.selectedTitle == value)
                {
                    return;
                }

                this.selectedTitle = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.SelectedTitleText));
                this.IsComboboxVisible = false;
                this.selectedTitleChanged?.Invoke();
            }
        }

        public string SelectedTitleText => this.selectedTitle.GetDescription();
    }
}