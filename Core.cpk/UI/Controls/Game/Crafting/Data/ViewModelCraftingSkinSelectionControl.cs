namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelCraftingSkinSelectionControl : BaseViewModel
    {
        private ViewModelCraftingMenuRecipeDetails selectedRecipeDetails;

        private IProtoItemWithSkinData selectedSkin;

        private IReadOnlyList<ViewModelSkinForCrafting> skins;

        public bool IsEditor => Api.IsEditor;

        public bool IsVisible => this.Skins is not null;

        public ViewModelCraftingMenuRecipeDetails SelectedRecipeDetails
        {
            get => this.selectedRecipeDetails;
            set
            {
                if (this.selectedRecipeDetails == value)
                {
                    return;
                }

                this.selectedRecipeDetails = value;
                this.NotifyThisPropertyChanged();
                this.RefreshSkinsList();
            }
        }

        public IProtoItemWithSkinData SelectedSkin
        {
            get => this.selectedSkin;
            set
            {
                if (this.selectedSkin == value)
                {
                    return;
                }

                this.selectedSkin = value;
                this.NotifyThisPropertyChanged();

                if (this.selectedRecipeDetails is not null)
                {
                    this.selectedRecipeDetails.SelectedSkin = value;
                }

                if (this.selectedSkin is null
                    || !this.selectedSkin.IsSkin)
                {
                    this.ViewModelSelectedSkin = null;
                    return;
                }

                this.ViewModelSelectedSkin?.Dispose();
                this.ViewModelSelectedSkin = new ViewModelSkin(this.selectedSkin,
                                                               Api.Client.Microtransactions.GetSkinData(
                                                                   (ushort)this.selectedSkin.SkinId));
            }
        }

        public IReadOnlyList<ViewModelSkinForCrafting> Skins
        {
            get => this.skins;
            private set
            {
                if (this.skins == value)
                {
                    return;
                }

                this.DisposeCollection(this.skins);
                this.skins = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.IsVisible));
            }
        }

        public ViewModelSkin ViewModelSelectedSkin { get; private set; }

        protected override void DisposeViewModel()
        {
            this.Skins = null;

            this.ViewModelSelectedSkin?.Dispose();
            this.ViewModelSelectedSkin = null;

            base.DisposeViewModel();
        }

        private void RefreshSkinsList()
        {
            this.DisposeCollection(this.skins);

            var skins = this.selectedRecipeDetails?.Skins;
            if (skins is null
                || skins.Count == 0)
            {
                this.Skins = null;
                this.SelectedSkin = null;
                return;
            }

            this.Skins = skins.Select(p => new ViewModelSkinForCrafting((IProtoItemWithSkinData)p))
                              .ToList();

            this.SelectedSkin = this.Skins[0].ProtoItemSkin;
        }
    }
}