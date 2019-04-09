namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowTrashCan : BaseUserControlWithWindow
    {
        private ObjectTrashCan.PrivateState privateState;

        private ViewModelWindowTrashCan viewModel;

        public static WindowTrashCan Show(ObjectTrashCan.PrivateState privateState)
        {
            var instance = new WindowTrashCan();
            Api.Client.UI.LayoutRootChildren.Add(instance);
            instance.Setup(privateState);
            return instance;
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
            if (this.viewModel == null)
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
                && this.WindowState != GameWindowState.Opening)
            {
                return;
            }

            if (this.viewModel != null)
            {
                if (this.privateState == this.viewModel.PrivateState)
                {
                    // already displayed
                    return;
                }

                this.DestroyViewModel();
            }

            if (this.privateState == null)
            {
                return;
            }

            this.viewModel = new ViewModelWindowTrashCan(this.privateState);
            this.DataContext = this.viewModel;
        }

        private void Setup(ObjectTrashCan.PrivateState privateState)
        {
            this.privateState = privateState;
            this.RefreshViewModel();
        }
    }
}