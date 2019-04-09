namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolPointer
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class EditorToolPointer : BaseEditorTool
    {
        public override string Name => "Pointer tool";

        public override int Order => 0;

        public override BaseEditorActiveTool Activate(BaseEditorToolItem item)
        {
            return new EditorToolPointerActive(this.ClientDeleteCallback);
        }

        private void ClientDeleteCallback(IReadOnlyCollection<IStaticWorldObject> worldObjectsToDelete)
        {
            EditorStaticObjectsRemovalHelper.ClientDelete(worldObjectsToDelete);
        }
    }
}