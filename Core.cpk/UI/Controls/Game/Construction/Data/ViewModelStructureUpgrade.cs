namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelStructureUpgrade : BaseViewModel
    {
        public ViewModelStructureUpgrade(IConstructionUpgradeEntryReadOnly upgradeEntry)
        {
            this.UpgradeEntry = upgradeEntry;
            this.ViewModelUpgradedStructure = new ViewModelStructure(upgradeEntry.ProtoStructure);

            this.Refresh();
            this.SubscribeToEvents();
        }

        public bool IsCanUpgrade { get; private set; }

        public bool IsTechLocked { get; set; }

        public IReadOnlyList<ProtoItemWithCount> RequiredItems => this.UpgradeEntry.RequiredItems;

        public IConstructionUpgradeEntryReadOnly UpgradeEntry { get; }

        public ViewModelStructure ViewModelUpgradedStructure { get; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            if (!IsDesignTime)
            {
                this.UnsubscribeFromEvents();
            }
        }

        private void ContainersItemsResetHandler()
        {
            this.Refresh();
        }

        private void ItemAddedOrRemovedOrCountChangedHandler(IItem item)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            var configUpgrade = this.UpgradeEntry;
            var value = configUpgrade.CheckRequirementsSatisfied(character);
            this.IsCanUpgrade = value;

            if (this.IsCanUpgrade)
            {
                this.IsTechLocked = false;
                return;
            }

            this.IsTechLocked = !configUpgrade.ProtoStructure.SharedIsTechUnlocked(character);
        }

        private void SubscribeToEvents()
        {
            ClientComponentTechnologiesWatcher.TechNodesChanged += this.TechNodesChangedHandler;
            ClientCurrentCharacterContainersHelper.ContainersItemsReset += this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged +=
                this.ItemAddedOrRemovedOrCountChangedHandler;
        }

        private void TechNodesChangedHandler()
        {
            this.Refresh();
        }

        private void UnsubscribeFromEvents()
        {
            ClientComponentTechnologiesWatcher.TechNodesChanged -= this.TechNodesChangedHandler;
            ClientCurrentCharacterContainersHelper.ContainersItemsReset -= this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged -=
                this.ItemAddedOrRemovedOrCountChangedHandler;
        }
    }
}