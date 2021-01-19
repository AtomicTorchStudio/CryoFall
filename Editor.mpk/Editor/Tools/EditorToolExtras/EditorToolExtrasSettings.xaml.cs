namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras
{
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class EditorToolExtrasSettings : BaseUserControl
    {
        public void Setup(ViewModelEditorToolExtras settingsViewModel)
        {
            this.DataContext = settingsViewModel;
        }
    }
}