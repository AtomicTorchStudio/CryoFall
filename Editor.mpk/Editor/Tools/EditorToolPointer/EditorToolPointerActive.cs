namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolPointer
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class EditorToolPointerActive : BaseEditorActiveTool
    {
        private readonly IClientSceneObject sceneObject;

        public EditorToolPointerActive(Action<IReadOnlyCollection<IStaticWorldObject>> deleteCallback)
        {
            this.sceneObject = Api.Client.Scene.CreateSceneObject("Editor Pointer Tool");
            this.sceneObject.AddComponent<ClientComponentEditorToolPointerActive>().Setup(deleteCallback);
        }

        public override void Dispose()
        {
            this.sceneObject.Destroy();
        }
    }
}