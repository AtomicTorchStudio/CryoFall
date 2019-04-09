namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CraftingRecipesListControl : BaseUserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ViewModelCraftingMenu),
                typeof(CraftingRecipesListControl),
                new PropertyMetadata(default(ViewModelCraftingMenu)));

        public CraftingRecipesListControl()
        {
        }

        public ViewModelCraftingMenu ViewModel
        {
            get => (ViewModelCraftingMenu)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        protected override void InitControl()
        {
        }
    }
}