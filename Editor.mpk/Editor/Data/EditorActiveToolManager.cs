namespace AtomicTorch.CBND.CoreMod.Editor.Data
{
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;

    public static class EditorActiveToolManager
    {
        private static BaseEditorActiveTool activeTool;

        public static BaseEditorActiveTool ActiveTool => activeTool;

        public static void Deactivate()
        {
            SetActiveTool(null, null);
        }

        public static void SetActiveTool(BaseEditorTool tool, BaseEditorToolItem item)
        {
            if (activeTool is not null)
            {
                activeTool.Dispose();
                activeTool = null;
            }

            if (tool is null)
            {
                return;
            }

            activeTool = tool.Activate(item);
        }
    }
}