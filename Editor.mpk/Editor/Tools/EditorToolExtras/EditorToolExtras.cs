namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;

    public class EditorToolExtras : BaseEditorTool
    {
        private ViewModelEditorToolExtras settingsViewModel;

        public override string Name => "Extras";

        public override int Order => 80;

        public override BaseEditorActiveTool Activate(BaseEditorToolItem item)
        {
            // there are no brush or any other active tool
            return null;
        }

        public override FrameworkElement CreateSettingsControl()
        {
            var control = new EditorToolExtrasSettings();
            this.settingsViewModel ??= new ViewModelEditorToolExtras();

            control.Setup(this.settingsViewModel);
            return control;
        }
    }
}