namespace AtomicTorch.CBND.CoreMod.Editor
{
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class EditorHUDLayoutControl : BaseUserControl
    {
        public EditorHUDLayoutControl()
        {
        }

        public static EditorHUDLayoutControl Instance { get; private set; }

        protected override void InitControl()
        {
            Instance = this;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.DataContext = new ViewModelEditorHUDLayoutControl();
        }

        protected override void OnUnloaded()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            ((ViewModelEditorHUDLayoutControl)this.DataContext).Dispose();
            this.DataContext = null;
        }
    }
}