namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolMap
{
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class EditorToolMapSettings : BaseUserControl
    {
        public EditorToolMapSettings()
        {
        }

        public void Setup(ViewModelEditorToolMapSettings settingsViewModel)
        {
            this.DataContext = settingsViewModel;
        }

        protected override void InitControl()
        {
        }
    }
}