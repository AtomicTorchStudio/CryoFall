namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelStructure : BaseViewModel
    {
        public readonly IProtoObjectStructure ProtoStructure;

        public ViewModelStructure(IProtoObjectStructure protoStructure)
        {
            this.ProtoStructure = protoStructure;

            if (IsDesignTime)
            {
                this.IsCanBuild = true;
                this.Icon = Brushes.BlueViolet;
                return;
            }

            if (protoStructure == null)
            {
                return;
            }

            this.UpdateIsCanBuild();
            this.SubscribeToContainersEvents();

            this.Icon = Client.UI.GetTextureBrush(this.ProtoStructure.Icon);
        }

#if !GAME

        public ViewModelStructure()
        {
            this.ProtoStructure = new ObjectCampfire();
        }

#endif

        public string Description => this.ProtoStructure.Description;

        public string DescriptionUpgrade => this.ProtoStructure.DescriptionUpgrade;

        public Brush Icon { get; }

        public bool IsCanBuild { get; private set; }

        public bool IsSelected { get; set; }

        public IReadOnlyList<ProtoItemWithCount> StageRequiredItems
            => this.ProtoStructure.ConfigBuild.StageRequiredItems;

        public byte StagesCount => this.ProtoStructure.ConfigBuild.StagesCount;

        public int StructurePointsMax => (int)this.ProtoStructure.StructurePointsMax;

        public string Title => this.ProtoStructure.Name;

        public Visibility VisibilityModeBlueprint { get; private set; } = Visibility.Visible;

        public Visibility VisibilityModeInstantBuild { get; private set; } = Visibility.Collapsed;

        public override string ToString()
        {
            return this.ProtoStructure.ToString();
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            if (!IsDesignTime)
            {
                this.UnsubscribeFromContainersEvents();
            }
        }

        private void ContainersItemsResetHandler()
        {
            this.UpdateIsCanBuild();
        }

        private void ItemAddedOrRemovedOrCountChangedHandler(IItem item)
        {
            this.UpdateIsCanBuild();
        }

        private void SubscribeToContainersEvents()
        {
            ClientCurrentCharacterContainersHelper.ContainersItemsReset += this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged +=
                this.ItemAddedOrRemovedOrCountChangedHandler;
        }

        private void UnsubscribeFromContainersEvents()
        {
            ClientCurrentCharacterContainersHelper.ContainersItemsReset -= this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged -=
                this.ItemAddedOrRemovedOrCountChangedHandler;
        }

        private void UpdateIsCanBuild()
        {
            var currentPlayerCharacter = Client.Characters.CurrentPlayerCharacter;
            var configBuild = this.ProtoStructure.ConfigBuild;
            var value = configBuild.CheckStageCanBeBuilt(currentPlayerCharacter);
            this.IsCanBuild = value;

            this.VisibilityModeBlueprint
                = configBuild.StagesCount > 1 ? Visibility.Visible : Visibility.Collapsed;

            this.VisibilityModeInstantBuild
                = configBuild.StagesCount == 1 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}