namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ConstructionTooltip
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Systems.Deconstruction;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ConstructionDeconstructTooltip : BaseUserControl
    {
        public static readonly DependencyProperty CanInteractProperty
            = DependencyProperty.Register(nameof(CanInteract),
                                          typeof(bool),
                                          typeof(ConstructionDeconstructTooltip),
                                          new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty TextActionProperty
            = DependencyProperty.Register(nameof(TextAction),
                                          typeof(string),
                                          typeof(ConstructionDeconstructTooltip),
                                          new PropertyMetadata(InteractionTooltipTexts.Deconstruct));

        private static readonly Color ColorMultiply
            = Color.FromArgb(0x60, 0xFF, 0x80, 0x80);

        private static readonly Color ColorOutline
            = Color.FromArgb(0xFF, 0xE5, 0x26, 0x40);

        private IComponentSpriteRenderer spriteRendererOutline;

        private IComponentSpriteRenderer worldObjectComponentSpriteRenderer;

        private EffectObjectOutline worldObjectComponentSpriteRendererEffect;

        public bool CanInteract
        {
            get => (bool)this.GetValue(CanInteractProperty);
            set => this.SetValue(CanInteractProperty, value);
        }

        public string TextAction
        {
            get => (string)this.GetValue(TextActionProperty);
            set => this.SetValue(TextActionProperty, value);
        }

        public IStaticWorldObject WorldObject { get; private set; }

        public static IComponentAttachedControl CreateAndAttach(IStaticWorldObject worldObject)
        {
            var control = new ConstructionDeconstructTooltip();
            control.WorldObject = worldObject;

            var positionOffset = worldObject.ProtoStaticWorldObject.SharedGetObjectCenterWorldOffset(worldObject);
            positionOffset += (0, 1.125);

            return Api.Client.UI.AttachControl(
                worldObject,
                control,
                positionOffset: positionOffset,
                isFocusable: true);
        }

        protected override void OnLoaded()
        {
            ClientUpdateHelper.UpdateCallback += this.Update;
            this.Update();
        }

        protected override void OnUnloaded()
        {
            ClientUpdateHelper.UpdateCallback -= this.Update;
            this.DestroyEffect();
        }

        private void DestroyEffect()
        {
            if (this.worldObjectComponentSpriteRenderer is null)
            {
                return;
            }

            this.worldObjectComponentSpriteRenderer.RemoveEffect(this.worldObjectComponentSpriteRendererEffect);
            this.worldObjectComponentSpriteRenderer = null;
            this.worldObjectComponentSpriteRendererEffect.Destroy();
            this.worldObjectComponentSpriteRendererEffect = null;

            this.spriteRendererOutline.Destroy();
            this.spriteRendererOutline = null;
        }

        private void Update()
        {
            if (!this.isLoaded)
            {
                return;
            }

            var canInteract = DeconstructionSystem.SharedCheckCanInteract(ClientCurrentCharacterHelper.Character,
                                                                          this.WorldObject,
                                                                          writeToLog: false);
            this.CanInteract = canInteract;

            var clientState = this.WorldObject.GetClientState<IClientStateWithObjectRenderer>();
            if (!canInteract
                || clientState is null
                || this.WorldObject.ProtoGameObject is ProtoObjectConstructionSite)
            {
                this.DestroyEffect();
                return;
            }

            if (clientState.Renderer == this.worldObjectComponentSpriteRenderer)
            {
                return;
            }

            this.DestroyEffect();

            if (clientState.Renderer is null
                || clientState.Renderer.Size.HasValue)
            {
                // no renderer or the renderer is using strictly configured size (such as farm plot; not supported)
                return;
            }

            this.worldObjectComponentSpriteRenderer = clientState.Renderer;
            this.worldObjectComponentSpriteRendererEffect = this.worldObjectComponentSpriteRenderer
                                                                ?.AddEffect<EffectObjectOutline>();

            this.worldObjectComponentSpriteRendererEffect.ColorOutline = ColorOutline;
            this.worldObjectComponentSpriteRendererEffect.ColorMultiply = ColorMultiply;

            this.spriteRendererOutline = Api.Client.Rendering.CreateSpriteRenderer(this.WorldObject);
            this.spriteRendererOutline.PositionOffset = this.worldObjectComponentSpriteRenderer.PositionOffset;
            this.spriteRendererOutline.SpritePivotPoint = this.worldObjectComponentSpriteRenderer.SpritePivotPoint;
            this.spriteRendererOutline.BlendMode = BlendMode.AlphaBlendPremultiplied;
            this.spriteRendererOutline.DrawOrder = DrawOrder.Overlay;

            this.worldObjectComponentSpriteRendererEffect.Setup(this.spriteRendererOutline,
                                                                this.worldObjectComponentSpriteRenderer.Scale);
        }
    }
}