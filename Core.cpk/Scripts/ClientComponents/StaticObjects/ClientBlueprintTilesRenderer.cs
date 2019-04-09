namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientBlueprintTilesRenderer
    {
        // we've decided to use the same color
        private static readonly EffectParameters EffectParameters =
            EffectParameters.Create()
                            .Set("Color", new Vector4(0.1f, 0.1f, 0.1f, 0.45f));

        private static RenderingMaterial renderingMaterial;

        private readonly IClientSceneObject sceneObjectRoot;

        private readonly List<IComponentSpriteRenderer> spriteRenderers
            = new List<IComponentSpriteRenderer>();

        private bool? isCanBuild;

        private bool isEnabled;

        public ClientBlueprintTilesRenderer(IClientSceneObject sceneObjectRoot)
        {
            this.sceneObjectRoot = sceneObjectRoot;

            if (renderingMaterial == null)
            {
                renderingMaterial = RenderingMaterial.Create(new EffectResource("SolidColor"));
                renderingMaterial.EffectParameters.Set(EffectParameters);
            }

            this.Reset();
        }

        public bool IsCanBuild
        {
            get => this.isCanBuild ?? false;
            set
            {
                if (this.isCanBuild == value)
                {
                    return;
                }

                this.isCanBuild = value;

                //var effectParameters = EffectParameters;
                //renderingMaterial.EffectParameters.Set(effectParameters);
            }
        }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }

                this.isEnabled = value;

                foreach (var componentSpriteRenderer in this.spriteRenderers)
                {
                    componentSpriteRenderer.IsEnabled = this.isEnabled;
                }
            }
        }

        public IClientSceneObject SceneObject { get; private set; }

        public void Reset()
        {
            this.SceneObject?.Destroy();
            this.SceneObject = Api.Client.Scene.CreateSceneObject("BlueprintTiles");

            this.SceneObject.AddComponent<SceneObjectPositionSynchronizer>()
                .Setup(this.sceneObjectRoot);
        }

        public void Setup(StaticObjectLayoutReadOnly layout)
        {
            if (this.spriteRenderers.Count > 0)
            {
                foreach (var componentSpriteRenderer in this.spriteRenderers)
                {
                    componentSpriteRenderer.Destroy();
                }

                this.spriteRenderers.Clear();
            }

            foreach (var layoutTileOffset in layout.TileOffsets)
            {
                this.AddSpriteRenderer(layoutTileOffset);
            }
        }

        private void AddSpriteRenderer(Vector2Int offset)
        {
            var spriteRenderer = Api.Client.Rendering.CreateSpriteRenderer(
                this.sceneObjectRoot,
                TextureResource.NoTexture,
                DrawOrder.Shadow + 1,
                positionOffset: offset.ToVector2D());
            spriteRenderer.RenderingMaterial = renderingMaterial;
            this.spriteRenderers.Add(spriteRenderer);
        }
    }
}