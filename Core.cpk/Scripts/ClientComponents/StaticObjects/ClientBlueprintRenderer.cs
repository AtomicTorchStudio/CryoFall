namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientBlueprintRenderer : IClientBlueprint
    {
        private static readonly EffectParameters EffectParametersAllow =
            EffectParameters.Create()
                            .Set("ColorAdd",      new Vector4(0,     0.25f, 0,     0))
                            .Set("ColorMultiply", new Vector4(0.75f, 1f,    0.75f, 0.85f));

        private static readonly EffectParameters EffectParametersDisallow =
            EffectParameters.Create()
                            .Set("ColorAdd",      new Vector4(0.3f, 0,     0,     0))
                            .Set("ColorMultiply", new Vector4(1.2f, 0.75f, 0.75f, 0.85f));

        private static readonly EffectParameters EffectParametersTooFar =
            EffectParameters.Create()
                            .Set("ColorAdd",      new Vector4(0.25f, 0.1f, 0,     0))
                            .Set("ColorMultiply", new Vector4(1.2f,  1f,   0.75f, 0.85f));

        private readonly IClientSceneObject sceneObjectRoot;

        private bool isEnabled = true;

        private EffectParameters lastEffectParameters;

        public ClientBlueprintRenderer(IClientSceneObject sceneObjectRoot)
        {
            this.sceneObjectRoot = sceneObjectRoot;
            this.SpriteRenderer = Api.Client.Rendering.CreateSpriteRenderer(
                sceneObjectRoot,
                TextureResource.NoTexture);

            this.SpriteRenderer.RenderingMaterial =
                RenderingMaterial.Create(new EffectResource("ConstructionPlacement"));

            this.Reset();
        }

        public bool IsCanBuild { get; set; }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }

                this.SpriteRenderer.IsEnabled = this.isEnabled = value;
            }
        }

        public bool IsTooFar { get; set; }

        public IClientSceneObject SceneObject { get; private set; }

        public IComponentSpriteRenderer SpriteRenderer { get; }

        public void RefreshMaterial()
        {
            EffectParameters effectParameters;
            if (this.IsCanBuild)
            {
                effectParameters = this.IsTooFar
                                       ? EffectParametersTooFar
                                       : EffectParametersAllow;
            }
            else
            {
                effectParameters = EffectParametersDisallow;
            }

            if (this.lastEffectParameters == effectParameters)
            {
                return;
            }

            this.lastEffectParameters = effectParameters;
            this.SpriteRenderer.RenderingMaterial.EffectParameters.Set(effectParameters);
        }

        public void Reset()
        {
            this.SceneObject?.Destroy();
            this.SceneObject = Api.Client.Scene.CreateSceneObject("Blueprint");

            this.SceneObject.AddComponent<SceneObjectPositionSynchronizer>()
                .Setup(this.sceneObjectRoot);

            this.SpriteRenderer.Reset();
            this.SpriteRenderer.DrawOrder = DrawOrder.Default + 1;
        }
    }
}