namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ConstructionTooltip;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

#if GAME
    using Vector4 = AtomicTorch.GameEngine.Common.Primitives.Vector4;
#endif

    public class ClientBlueprintRenderer : IClientBlueprint
    {
        private const double TextBlockCannotBuildReasonOffsetY = 1.125;

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

        private readonly Vector2D textBlockCannotBuildReasonOffset;

        private string cannotBuildReason;

        private bool isEnabled = true;

        private EffectParameters lastEffectParameters;

        private CannotBuildReasonControl textBlockCannotBuildReason;

        public ClientBlueprintRenderer(
            IClientSceneObject sceneObjectRoot,
            bool isConstructionSite,
            Vector2D centerOffset)
        {
            this.sceneObjectRoot = sceneObjectRoot;
            this.IsConstructionSite = isConstructionSite;
            this.textBlockCannotBuildReasonOffset = centerOffset + (0, TextBlockCannotBuildReasonOffsetY);

            this.SpriteRenderer = Api.Client.Rendering.CreateSpriteRenderer(
                sceneObjectRoot,
                TextureResource.NoTexture);

            this.SpriteRenderer.RenderingMaterial =
                RenderingMaterial.Create(new EffectResource("ConstructionPlacement"));

            this.Reset();
        }

        public string CannotBuildReason
        {
            get => this.cannotBuildReason;
            set
            {
                if (this.cannotBuildReason == value)
                {
                    return;
                }

                this.cannotBuildReason = value;

                if (string.IsNullOrEmpty(this.cannotBuildReason))
                {
                    if (this.textBlockCannotBuildReason is not null)
                    {
                        this.textBlockCannotBuildReason.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    this.CreateTextBlockControlIfNecessary();
                    this.textBlockCannotBuildReason.Text = this.cannotBuildReason.TrimEnd('.', '。');
                    this.textBlockCannotBuildReason.Visibility = Visibility.Visible;
                    this.textBlockCannotBuildReason.IsWarning = this.IsTooFar;
                }
            }
        }

        public bool IsCanBuild { get; set; }

        public bool IsConstructionSite { get; }

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

                if (!this.isEnabled)
                {
                    this.Reset();
                    this.CannotBuildReason = null;
                }
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

        private void CreateTextBlockControlIfNecessary()
        {
            if (this.textBlockCannotBuildReason is not null)
            {
                return;
            }

            this.textBlockCannotBuildReason = new CannotBuildReasonControl();
            Api.Client.UI.AttachControl(this.sceneObjectRoot,
                                        this.textBlockCannotBuildReason,
                                        positionOffset: this.textBlockCannotBuildReasonOffset,
                                        isFocusable: false,
                                        isScaleWithCameraZoom: true);
        }
    }
}