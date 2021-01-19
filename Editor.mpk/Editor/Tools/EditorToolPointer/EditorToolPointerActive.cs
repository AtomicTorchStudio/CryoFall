namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolPointer
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorToolPointerActive : BaseEditorActiveTool
    {
        private readonly IClientSceneObject sceneObject;

        public EditorToolPointerActive()
        {
            this.sceneObject = Api.Client.Scene.CreateSceneObject("Editor Pointer Tool");
            this.sceneObject.AddComponent<ClientComponentEditorToolPointerActive>()
                .Setup(ClientDeleteCallback,
                       ClientCopyCallback,
                       ClientPasteCallback);
        }

        public override void Dispose()
        {
            this.sceneObject.Destroy();
            EditorStaticObjectsCopyPasteHelper.DestroyInstance();
        }

        private static void ClientCopyCallback(IReadOnlyCollection<IStaticWorldObject> worldObjectsToCopy)
        {
            EditorStaticObjectsCopyPasteHelper.ClientCopy(worldObjectsToCopy);
        }

        private static void ClientDeleteCallback(IReadOnlyCollection<IStaticWorldObject> worldObjectsToDelete)
        {
            EditorStaticObjectsRemovalHelper.ClientDelete(worldObjectsToDelete);
        }

        private static void ClientPasteCallback(Vector2Ushort tilePosition)
        {
            EditorStaticObjectsCopyPasteHelper.ClientPaste(tilePosition);
        }
    }
}