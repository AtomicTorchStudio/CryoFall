namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelCraftingRecipe : BaseViewModel
    {
        public readonly Recipe Recipe;

        public ViewModelCraftingRecipe(Recipe recipe)
        {
            this.Recipe = recipe;
        }

        public string DurationText
        {
            get
            {
                var duration = this.Recipe.SharedGetDurationForPlayer(ClientCurrentCharacterHelper.Character);
                return ClientTimeFormatHelper.FormatTimeDuration(duration);
            }
        }

        public TextureBrush Icon => Api.Client.UI.GetTextureBrush(this.Recipe.Icon);

        public string Title => this.Recipe.Name;

        public override string ToString()
        {
            return this.Recipe?.ToString() ?? string.Empty;
        }
    }
}