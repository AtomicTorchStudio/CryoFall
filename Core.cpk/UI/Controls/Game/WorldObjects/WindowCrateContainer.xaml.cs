namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowCrateContainer : BaseUserControlWithWindow
    {
        private ObjectCratePrivateState privateState;

        private ViewModelWindowCrateContainer viewModel;

        private IStaticWorldObject worldObject;

        public static WindowCrateContainer Show(IStaticWorldObject worldObject, ObjectCratePrivateState privateState)
        {
            var instance = new WindowCrateContainer();
            Api.Client.UI.LayoutRootChildren.Add(instance);
            instance.Setup(worldObject, privateState);
            return instance;
        }

        public void Close()
        {
            this.CloseWindow();
        }

        protected override void InitControlWithWindow()
        {
            // TODO: redone this to cached window when NoesisGUI implement proper Storyboard.Completed triggers
            this.Window.IsCached = false;
        }

        protected override void WindowClosing()
        {
            this.DestroyViewModel();
        }

        protected override void WindowOpening()
        {
            this.RefreshViewModel();
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

        private void RefreshViewModel()
        {
            if (this.WindowState != GameWindowState.Opened
                && this.WindowState != GameWindowState.Opening
                || this.worldObject is null)
            {
                return;
            }

            if (this.viewModel is not null)
            {
                if (this.viewModel.WorldObjectCrate == this.worldObject)
                {
                    // already displaying window for this container
                    return;
                }

                this.DestroyViewModel();
            }

            this.viewModel = new ViewModelWindowCrateContainer(this.worldObject, this.privateState, this.Close);
            this.DataContext = this.viewModel;
        }

        private void Setup(IStaticWorldObject worldObject, ObjectCratePrivateState privateState)
        {
            this.worldObject = worldObject;
            this.privateState = privateState;
            this.RefreshViewModel();
        }
    }
}