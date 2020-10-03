namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentEditorToolActiveTileBrush : ClientComponent
    {
        public static readonly ITextureResource BlueprintTexture = new TextureResource("Editor/TileBlueprint");

        private static readonly EffectParameters EffectParametersDefault =
            EffectParameters.Create()
                            .Set("ColorMultiply", new Vector4(1f, 1f, 1f, 1f));

        private static readonly EffectParameters EffectParametersGreen =
            EffectParameters.Create()
                            .Set("ColorMultiply", new Vector4(0.25f, 1f, 0.25f, 1f));

        private static readonly EffectParameters EffectParametersRed =
            EffectParameters.Create()
                            .Set("ColorMultiply", new Vector4(1f, 0f, 0f, 1f));

        private Vector2Int[] customBrushOffsets;

        private Action inputReleasedCallback;

        private bool isInitialized;

        private bool isMouseButtonHeld;

        private bool isRepeatOnMove;

        private RenderingMaterial material;

        private ApplyToolDelegate selectedCallback;

        private Func<List<Vector2Ushort>, bool> validateCallback;

        public EditorBrushShape Brush { get; private set; }

        public int BrushSize { get; private set; }

        public bool IsReactOnRightMouseButton { get; set; }

        public void SetBrush(EditorBrushShape brush, int brushSize)
        {
            this.Brush = brush;
            this.BrushSize = brushSize;
            this.isInitialized = false;
        }

        public void SetCustomBrush(Vector2Int[] offsets)
        {
            this.customBrushOffsets = offsets;
        }

        public void Setup(
            ApplyToolDelegate selectedCallback,
            Func<List<Vector2Ushort>, bool> validateCallback,
            bool isRepeatOnMove,
            Action inputReleasedCallback = null)
        {
            this.selectedCallback = selectedCallback;
            this.inputReleasedCallback = inputReleasedCallback;
            this.validateCallback = validateCallback;
            this.isRepeatOnMove = isRepeatOnMove;
            this.material = RenderingMaterial.Create(new EffectResource("ConstructionBlueprint"));
            this.material.EffectParameters.Set(EffectParametersDefault);
        }

        public override void Update(double deltaTime)
        {
            var isUpdateRequired = false;
            if (!this.isInitialized)
            {
                // first update called
                this.InititializeBlueprint();
                isUpdateRequired = true;
            }

            var tilePosition = Client.Input.MousePointedTilePosition;
            var tilePositionVector2D = tilePosition.ToVector2D();

            var isPositionChanged = this.SceneObject.Position != tilePositionVector2D;
            if (isPositionChanged
                || isUpdateRequired)
            {
                // position changed, update sprite renderer
                this.SceneObject.Position = tilePositionVector2D;
                this.OnPositionChanged();
                this.UpdateBlueprint(tilePosition);
            }

            if (this.isMouseButtonHeld
                && isPositionChanged
                && this.isRepeatOnMove)
            {
                // moved mouse while holding key
                this.OnSelected(isRepeat: true);
            }
            else if (ClientInputManager.IsButtonDown(GameButton.ActionUseCurrentItem)
                     || this.IsReactOnRightMouseButton
                     && ClientInputManager.IsButtonDown(GameButton.ActionInteract))
            {
                // pressed mouse button
                this.OnSelected(isRepeat: false);
                this.isMouseButtonHeld = true;
            }

            if (ClientInputManager.IsButtonUp(GameButton.ActionUseCurrentItem)
                || this.IsReactOnRightMouseButton
                && ClientInputManager.IsButtonUp(GameButton.ActionInteract))
            {
                this.isMouseButtonHeld = false;
                this.inputReleasedCallback?.Invoke();
            }
        }

        private List<Vector2Ushort> CalculateTilePositions()
        {
            var position = this.SceneObject.Position.ToVector2Int();
            var offsets = this.GetBrushOffsets(isForPreview: false);

            var worldBounds = Client.World.WorldBounds;

            var positions = offsets.Select(o => position + o)
                                   .Where(p => worldBounds.Contains(p.X, p.Y))
                                   .Select(p => (Vector2Ushort)p)
                                   .ToList();
            return positions;
        }

        private IEnumerable<Vector2Int> GetBrushOffsets(bool isForPreview)
        {
            return this.customBrushOffsets
                   ?? EditorTileOffsetsHelper.GenerateOffsets(this.Brush, this.BrushSize, isForPreview);
        }

        private void InititializeBlueprint()
        {
            // create blueprint renderer
            var texture = BlueprintTexture;

            this.isInitialized = true;

            foreach (var componentSpriteRenderer in this.SceneObject.FindComponents<IComponentSpriteRenderer>())
            {
                componentSpriteRenderer.Destroy();
            }

            foreach (var offset in this.GetBrushOffsets(isForPreview: true))
            {
                var spriteRendererWorldObject = Client.Rendering.CreateSpriteRenderer(
                    this.SceneObject,
                    texture,
                    DrawOrder.Default + 1,
                    positionOffset: (Vector2D)offset);
                spriteRendererWorldObject.RenderingMaterial = this.material;
            }
        }

        private void OnPositionChanged()
        {
            if (this.validateCallback is null)
            {
                return;
            }

            var positions = this.CalculateTilePositions();
            if (positions.Count > 0)
            {
                var result = this.validateCallback(positions);
                this.material.EffectParameters.Set(result ? EffectParametersGreen : EffectParametersRed);
            }
        }

        private void OnSelected(bool isRepeat)
        {
            var positions = this.CalculateTilePositions();
            if (positions.Count == 0)
            {
                return;
            }

            if (this.validateCallback is null
                || this.validateCallback(positions))
            {
                this.selectedCallback(positions, isRepeat);
            }
        }

        private void UpdateBlueprint(Vector2Ushort tilePosition)
        {
            return;
        }
    }
}