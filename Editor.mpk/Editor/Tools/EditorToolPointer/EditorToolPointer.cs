namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolPointer
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class EditorToolPointer : BaseEditorTool
    {
        public override string Name => "Pointer tool";

        public override int Order => 0;

        public override BaseEditorActiveTool Activate(BaseEditorToolItem item)
        {
            return new EditorToolPointerActive();
        }

        public override FrameworkElement CreateSettingsControl()
        {
            return new FormattedTextBlock()
            {
                Content =
                    @"Hold Shift or Ctrl to add objects to selection.
                      [br]Hold Alt to exclude objects from selection.
                      [br]Press Del to delete selected object.
                      [br]Use Ctrl+C/X/V to copy/cut/paste objects.
                      [br]Use Ctrl+Z to undo, Ctrl+Y to redo.",
                Foreground = Brushes.White,
                FontSize = 11
            };
        }
    }
}