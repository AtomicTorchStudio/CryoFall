namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CraftingQueueControl : BaseUserControl
    {
        private CraftingQueue craftingQueue;

        private ViewModelCraftingQueue viewModel;

        public static CraftingQueueControl Instance { get; private set; }

        public void Refresh()
        {
            this.craftingQueue = ClientCurrentCharacterHelper.PrivateState?.CraftingQueue;
            this.DestroyViewModel();

            if (this.craftingQueue is not null
                && this.isLoaded)
            {
                this.CreateViewModel();
            }
        }

        protected override void InitControl()
        {
            Instance = this;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            this.DestroyViewModel();
        }

        private void CreateViewModel()
        {
            this.DestroyViewModel();

            var children = this.GetByName<StackPanel>("CraftingQueueItems").Children;
            children.Clear();
            this.DataContext = this.viewModel = new ViewModelCraftingQueue(this.craftingQueue, children);
        }

        private void DestroyViewModel()
        {
            if (this.viewModel is null)
            {
                return;
            }

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}