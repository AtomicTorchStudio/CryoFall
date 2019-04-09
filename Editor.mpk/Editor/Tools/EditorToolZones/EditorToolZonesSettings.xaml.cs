namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class EditorToolZonesSettings : BaseUserControl
    {
        public ViewModelEditorToolZonesSettings ViewModel { get; private set; }

        public void Setup(ViewModelEditorToolZonesSettings viewModel)
        {
            this.ViewModel = viewModel;
            if (this.isLoaded)
            {
                this.DataContext = this.ViewModel;
            }
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            if (this.ViewModel != null)
            {
                this.DataContext = this.ViewModel;
            }
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
        }
    }
}