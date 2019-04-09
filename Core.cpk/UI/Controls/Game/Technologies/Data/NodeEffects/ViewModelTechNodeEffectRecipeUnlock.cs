namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelTechNodeEffectRecipeUnlock : BaseViewModelTechNodeEffect
    {
        public const string TitleHandCrafting = "Hand crafting";

        private readonly TechNodeEffectRecipeUnlock effect;

        public ViewModelTechNodeEffectRecipeUnlock(TechNodeEffectRecipeUnlock effect) : base(effect)
        {
            this.effect = effect;
        }

        public string StationTitle
        {
            get
            {
                if (!(this.effect.Recipe is Recipe.BaseRecipeForStation recipeForStation))
                {
                    return null;
                }

                var result = GetStationTitleInternal(recipeForStation);
                var isHandCrafting = this.effect.Recipe is Recipe.RecipeForHandCrafting;
                if (!isHandCrafting)
                {
                    return result;
                }

                // prepend handcrafting title
                if (string.IsNullOrEmpty(result))
                {
                    return TitleHandCrafting;
                }

                return TitleHandCrafting + ", " + result;
            }
        }

        public string Title => this.effect.Recipe.Name;

        public Visibility VisibilityStationTitle => this.effect.Recipe is Recipe.BaseRecipeForStation
                                                        ? Visibility.Visible
                                                        : Visibility.Collapsed;

        private static string GetStationTitleInternal(Recipe.BaseRecipeForStation recipeForStation)
        {
            var stations = recipeForStation.StationTypes;
            switch (stations.Count)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return stations[0].Name;
                default:
                    return stations.Select(s => s.Name).GetJoinedString();
            }
        }
    }
}