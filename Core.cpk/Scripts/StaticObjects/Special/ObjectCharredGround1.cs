namespace AtomicTorch.CBND.CoreMod.StaticObjects.Special
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special.Base;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ObjectCharredGround1 : ProtoObjectCharredGround
    {
        // TODO: replace it with a simple shared blending between two sprites
        public static readonly ITextureAtlasResource ExplosionGroundTextureAtlas
            = new TextureAtlasResource("FX/Explosions/ExplosionGround1",
                                       columns: 8,
                                       rows: 3,
                                       isTransparent: true);

        [NotLocalizable]
        public override string Name => "Charred ground";

        public static void ClientPreloadContent()
        {
            Api.Client.Rendering.PreloadTextureAsync(ExplosionGroundTextureAtlas);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var tilePosition = data.GameObject.TilePosition;
            var renderer = data.ClientState.Renderer;

            if (!ClientGroundExplosionAnimationHelper.HasActiveExplosion(tilePosition))
            {
                return;
            }

            // this is a fresh charred ground, animate the ground sprite
            var animationDuration = ClientGroundExplosionAnimationHelper.ExplosionGroundDuration;
            var frames = ClientComponentSpriteSheetAnimator.CreateAnimationFrames(ExplosionGroundTextureAtlas);

            var componentAnimator = renderer.SceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
            componentAnimator.Setup(renderer,
                                    frames,
                                    isLooped: false,
                                    frameDurationSeconds: animationDuration / frames.Length);

            componentAnimator.Destroy(1.5 * animationDuration);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return ExplosionGroundTextureAtlas.Chunk(7, 2);
        }
    }
}