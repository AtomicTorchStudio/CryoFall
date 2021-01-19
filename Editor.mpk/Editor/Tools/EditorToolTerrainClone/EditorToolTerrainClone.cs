namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrainClone
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class EditorToolTerrainClone : BaseEditorTool
    {
        public override string Name => "Terrain clone tool";

        public override int Order => 12;

        public override BaseEditorActiveTool Activate(BaseEditorToolItem item)
        {
            return new EditorToolTerrainCloneActive();
        }

        public override FrameworkElement CreateSettingsControl()
        {
            return new FormattedTextBlock()
            {
                Content =
                    @"Move cursor while holding LMB to move selection rectangle.
                      [br]Drag selection rectangle by its border to expand the selection.                       
                      [br]Use Ctrl+C to copy, Ctrl+V to paste.
                      [br]Use Ctrl+Z to undo, Ctrl+Y to redo.",
                Foreground = Brushes.White,
                FontSize = 11
            };
        }
    }
}