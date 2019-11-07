namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Environment
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectLavaCrack : ProtoObjectMisc, IProtoObjectHeatSource
    {
        private Vector2D textureAltasAnimationDrawPositionWorldOffset;

        private ITextureAtlasResource textureAtlasAnimation;

        private double textureAtlasAnimationFrameDurationSeconds;

        public override bool CanFlipSprite => false;

        public virtual double HeatIntensity => 1;

        public abstract double HeatRadiusMax { get; }

        public abstract double HeatRadiusMin { get; }

        // define this object as a structure to prevent terrain decals rendered under it
        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public override string Name => "Fissure";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var renderer = data.ClientState.Renderer;
            renderer.DrawOrder = DrawOrder.Floor;

            var overlayRenderer = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                TextureResource.NoTexture);

            this.ClientSetupRenderer(overlayRenderer);

            overlayRenderer.PositionOffset = renderer.PositionOffset
                                             + this.textureAltasAnimationDrawPositionWorldOffset;
            overlayRenderer.SpritePivotPoint = renderer.SpritePivotPoint;
            overlayRenderer.Scale = renderer.Scale;
            overlayRenderer.DrawOrder = renderer.DrawOrder + 1;

            data.GameObject
                .ClientSceneObject
                .AddComponent<ClientComponentSpriteSheetBlendAnimator>()
                .Setup(
                    overlayRenderer,
                    ClientComponentSpriteSheetAnimator.CreateAnimationFrames(this.textureAtlasAnimation),
                    frameDurationSeconds: this.textureAtlasAnimationFrameDurationSeconds);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            this.textureAtlasAnimation =
                this.PrepareTextureAtlasAnimation(out var animationFrameDurationSeconds,
                                                  out var animationDrawPositionWorldOffset);
            this.textureAtlasAnimationFrameDurationSeconds = animationFrameDurationSeconds;
            this.textureAltasAnimationDrawPositionWorldOffset = animationDrawPositionWorldOffset;

            return base.PrepareDefaultTexture(thisType);
        }

        protected abstract ITextureAtlasResource PrepareTextureAtlasAnimation(
            out double frameDurationSeconds,
            out Vector2D drawPositionWorldOffset);
    }
}