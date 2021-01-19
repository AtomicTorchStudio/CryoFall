namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipFoodNutrition : BaseUserControl
    {
        public static ItemTooltipFoodNutrition Create(IProtoItemFood item)
        {
            var data = new FoodNutritionValueData();
            data.Add(item, count: 1);
            return new ItemTooltipFoodNutrition() { DataContext = data };
        }
    }
}