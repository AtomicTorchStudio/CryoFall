namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrain
{
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class EditorToolTerrainSettings : BaseUserControl
    {
        public ViewModelEditorToolTerrainSettings ViewModel { get; private set; }

        public void Setup(ViewModelEditorToolTerrainSettings viewModel)
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
            if (this.ViewModel is not null)
            {
                this.DataContext = this.ViewModel;
            }
        }
    }
}