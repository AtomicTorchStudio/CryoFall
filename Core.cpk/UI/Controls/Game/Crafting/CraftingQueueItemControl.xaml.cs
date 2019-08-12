namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CraftingQueueItemControl : BaseUserControl
    {
        public static readonly DependencyProperty RequiredHeightProperty =
            DependencyProperty.Register(nameof(RequiredHeight),
                                        typeof(float),
                                        typeof(CraftingQueueItemControl),
                                        new PropertyMetadata(default(float)));

        public Action<CraftingQueueItem> CountToCraftChangedCallback;

        public Action<CraftingQueueItem> DeleteCallback;

        public Action<CraftingQueueItemControl> HiddenCallback;

        public Action<CraftingQueueItem> MakeFirstCallback;

        private Border border;

        private CraftingQueue craftingQueue;

        private CraftingQueueItem craftingQueueItem;

        private Storyboard storyboardFadeOut;

        private Storyboard storyboardHide;

        private Storyboard storyboardShow;

        private ViewModelCraftingQueueItem viewModel;

        public bool IsHiding { get; private set; }

        public float RequiredHeight
        {
            get => (float)this.GetValue(RequiredHeightProperty);
            set => this.SetValue(RequiredHeightProperty, value);
        }

        public void Hide()
        {
            if (this.IsHiding)
            {
                // already hiding
                return;
            }

            this.IsHiding = true;

            if (!this.isLoaded)
            {
                this.RemoveControl();
                return;
            }

            this.storyboardShow.Stop();
            this.storyboardFadeOut.Begin();
        }

        public void Setup(CraftingQueue craftingQueue, CraftingQueueItem craftingQueueItem)
        {
            this.craftingQueue = craftingQueue;
            this.craftingQueueItem = craftingQueueItem;
            this.Refresh();
        }

        protected override void InitControl()
        {
            this.border = this.GetByName<Border>("Border");
            this.storyboardShow = this.GetResource<Storyboard>("StoryboardShow");
            this.storyboardHide = this.GetResource<Storyboard>("StoryboardHide");
            this.storyboardFadeOut = this.GetResource<Storyboard>("StoryboardFadeOut");
        }

        protected override void OnLoaded()
        {
            this.RequiredHeight = (float)this.ActualHeight;

            this.storyboardFadeOut.Completed += this.StoryboardFadeOutCompletedHandler;
            this.storyboardHide.Completed += this.StoryboardHideCompletedHandler;
            this.MouseLeftButtonUp += this.MouseLeftButtonUpHandler;
            this.MouseRightButtonUp += this.MouseRightButtonUpHandler;

            this.Refresh();

            this.storyboardShow.Begin();
        }

        protected override void OnUnloaded()
        {
            this.storyboardFadeOut.Completed -= this.StoryboardFadeOutCompletedHandler;
            this.storyboardHide.Completed -= this.StoryboardHideCompletedHandler;
            this.MouseLeftButtonUp -= this.MouseLeftButtonUpHandler;
            this.MouseRightButtonUp -= this.MouseRightButtonUpHandler;
            this.Refresh();

            // invoke on the next frame
            ClientTimersSystem.AddAction(0, () => this.HiddenCallback?.Invoke(this));
        }

        private void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            this.MakeFirstCallback?.Invoke(this.craftingQueueItem);
        }

        private void MouseRightButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            this.DeleteCallback?.Invoke(this.craftingQueueItem);
        }

        private void OnCountToCraftChangedCallback()
        {
            this.CountToCraftChangedCallback?.Invoke(this.craftingQueueItem);
        }

        private void Refresh()
        {
            if (!this.isLoaded
                || this.craftingQueueItem == null)
            {
                if (this.viewModel == null)
                {
                    return;
                }

                this.border.DataContext = null;
                this.viewModel.Dispose();
                this.viewModel = null;
                return;
            }

            if (this.viewModel == null)
            {
                this.viewModel = new ViewModelCraftingQueueItem(
                    this.craftingQueue,
                    this.craftingQueueItem,
                    this.OnCountToCraftChangedCallback);
            }

            this.border.DataContext = this.viewModel;
        }

        private void RemoveControl()
        {
            ((Panel)this.Parent).Children.Remove(this);
        }

        private void StoryboardFadeOutCompletedHandler(object sender, EventArgs e)
        {
            this.storyboardHide.Begin();
        }

        private void StoryboardHideCompletedHandler(object sender, EventArgs e)
        {
            this.RemoveControl();
        }
    }
}