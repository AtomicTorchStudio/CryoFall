namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class EditorActiveToolObjectBrush : BaseEditorActiveTool
    {
        private readonly Action onDispose;

        private readonly Action<List<Vector2Ushort>> onSelected;

        private readonly IProtoStaticWorldObject protoStaticObject;

        private readonly IClientSceneObject sceneObject;

        public EditorActiveToolObjectBrush(
            IProtoStaticWorldObject protoStaticObject,
            [NotNull] Action<List<Vector2Ushort>> onSelected,
            Action onDispose = null)
        {
            this.protoStaticObject = protoStaticObject;
            this.onSelected = onSelected;
            this.onDispose = onDispose;

            this.sceneObject = Api.Client.Scene.CreateSceneObject("ActiveEditorToolBrush", Vector2D.Zero);
            this.sceneObject.AddComponent<ClientComponentObjectPlacementHelper>()
                .Setup(
                    protoStaticObject,
                    isCancelable: false,
                    isRepeatCallbackIfHeld: true,
                    isDrawConstructionGrid: false,
                    isBlockingInput: false,
                    validateCanPlaceCallback: this.ValidateCanBuild,
                    placeSelectedCallback: this.PlaceSelectedHandler);
        }

        public override void Dispose()
        {
            this.sceneObject.Destroy();
            this.onDispose?.Invoke();
        }

        private void PlaceSelectedHandler(Vector2Ushort tilePosition)
        {
            this.onSelected(
                new List<Vector2Ushort>()
                {
                    tilePosition
                });
        }

        private void ValidateCanBuild(
            Vector2Ushort tilePosition,
            bool logErrors,
            out bool canPlace,
            out bool isTooFar)
        {
            isTooFar = false;
            canPlace = this.protoStaticObject.CheckTileRequirements(tilePosition,
                                                                    character: null,
                                                                    logErrors: logErrors);
        }
    }
}