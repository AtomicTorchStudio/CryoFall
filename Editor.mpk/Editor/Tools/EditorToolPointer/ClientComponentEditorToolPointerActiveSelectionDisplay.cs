namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolPointer
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentEditorToolPointerActiveSelectionDisplay : ClientComponent
    {
        public static readonly ITextureResource SelectionTexture = new TextureResource("Editor/SelectedTileOverlay");

        private readonly Dictionary<Vector2Ushort, List<IStaticWorldObject>> selectedTileObjects =
            new Dictionary<Vector2Ushort, List<IStaticWorldObject>>();

        private readonly IDictionary<Vector2Ushort, IClientSceneObject> tileRenderers =
            new Dictionary<Vector2Ushort, IClientSceneObject>();

        public void Deselect(IStaticWorldObject staticWorldObject)
        {
            foreach (var tilePosition in staticWorldObject.OccupiedTilePositions)
            {
                if (!this.selectedTileObjects.TryGetValue(tilePosition, out var list)
                    || !list.Remove(staticWorldObject))
                {
                    throw new Exception("Not found in the selection: " + staticWorldObject);
                }

                if (list.Count == 0)
                {
                    this.selectedTileObjects.Remove(tilePosition);
                    this.DestroySelectionRenderer(tilePosition);
                }
            }
        }

        public void Select(IStaticWorldObject staticWorldObject)
        {
            foreach (var tilePosition in staticWorldObject.OccupiedTilePositions)
            {
                var list = this.selectedTileObjects.AddToValueList(tilePosition, staticWorldObject);
                if (list.Count == 1)
                {
                    this.CreateSelectionRenderer(tilePosition);
                }
            }
        }

        protected override void OnDisable()
        {
            foreach (var sceneObject in this.tileRenderers.Values)
            {
                sceneObject.Destroy();
            }

            this.tileRenderers.Clear();
            this.selectedTileObjects.Clear();
        }

        private void CreateSelectionRenderer(Vector2Ushort tilePosition)
        {
            var rendererSceneObject = Client.Scene.CreateSceneObject("Selection rendering");
            Client.Rendering.CreateSpriteRenderer(
                rendererSceneObject,
                SelectionTexture,
                DrawOrder.Overlay);
            rendererSceneObject.Position = tilePosition.ToVector2D();

            this.tileRenderers.Add(tilePosition, rendererSceneObject);
        }

        private void DestroySelectionRenderer(Vector2Ushort tilePosition)
        {
            var rendererSceneObject = this.tileRenderers[tilePosition];
            this.tileRenderers.Remove(tilePosition);
            rendererSceneObject.Destroy();
        }
    }
}