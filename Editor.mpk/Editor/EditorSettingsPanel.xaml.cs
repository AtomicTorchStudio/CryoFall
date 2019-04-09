namespace AtomicTorch.CBND.CoreMod.Editor
{
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class EditorSettingsPanel : BaseUserControl
    {
        public EditorSettingsPanel()
        {
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = ViewModelEditorSettingsPanel.Instance;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
        }
    }
}