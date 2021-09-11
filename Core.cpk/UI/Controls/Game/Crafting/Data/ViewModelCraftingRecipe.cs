namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelCraftingRecipe : BaseViewModel
    {
        public ViewModelCraftingRecipe(Recipe recipe)
        {
            this.Recipe = recipe;
            ClientCurrentCharacterFinalStatsHelper.FinalStatsCacheChanged += this.RefreshDurationText;
        }

        public string DurationText
        {
            get
            {
                var character = ClientCurrentCharacterHelper.Character;
                var duration = this.Recipe.SharedGetDurationForPlayer(character, shortenForCreativeMode: false);

                if (InteractionCheckerSystem.SharedGetCurrentInteraction(character)?.ProtoWorldObject is
                        IProtoObjectManufacturer protoObjectManufacturer)
                {
                    duration /= protoObjectManufacturer.ManufacturingSpeedMultiplier;
                }

                return ClientTimeFormatHelper.FormatTimeDuration(duration,
                                                                 roundSeconds: false);
            }
        }

        public TextureBrush Icon => Api.Client.UI.GetTextureBrush(this.Recipe.Icon);

        public Recipe Recipe { get; }

        public string Title => this.Recipe.Name;

        public override string ToString()
        {
            return this.Recipe?.ToString() ?? string.Empty;
        }

        protected override void DisposeViewModel()
        {
            ClientCurrentCharacterFinalStatsHelper.FinalStatsCacheChanged -= this.RefreshDurationText;
            base.DisposeViewModel();
        }

        private void RefreshDurationText()
        {
            this.NotifyPropertyChanged(nameof(this.DurationText));
        }
    }
}