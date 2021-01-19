namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrainClone
{
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorToolTerrainCloneActive : BaseEditorActiveTool
    {
        private readonly IClientSceneObject sceneObject;

        public EditorToolTerrainCloneActive()
        {
            this.sceneObject = Api.Client.Scene.CreateSceneObject("Editor Terrain Clone Tool");
            this.sceneObject.AddComponent<ClientComponentEditorToolTerrainClone>()
                .Setup(ClientCopyCallback,
                       ClientPasteCallback);
        }

        public override void Dispose()
        {
            this.sceneObject.Destroy();
            EditorTerrainCopyPasteHelper.DestroyInstance();
        }

        private static void ClientCopyCallback(BoundsUshort bounds)
        {
            EditorTerrainCopyPasteHelper.ClientCopy(bounds);
        }

        private static void ClientPasteCallback(Vector2Ushort tilePosition)
        {
            EditorTerrainCopyPasteHelper.ClientPaste(tilePosition);
        }
    }
}