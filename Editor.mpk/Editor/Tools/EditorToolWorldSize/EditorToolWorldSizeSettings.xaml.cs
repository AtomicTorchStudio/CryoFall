namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolWorldSize
{
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class EditorToolWorldSizeSettings : BaseUserControl
    {
        public void Setup(ViewModelEditorToolWorldSizeSettings settingsViewModel)
        {
            this.DataContext = settingsViewModel;
        }

        protected override void InitControl()
        {
        }
    }
}