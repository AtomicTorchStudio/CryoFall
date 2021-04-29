namespace AtomicTorch.CBND.CoreMod.StaticObjects.Special
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special.Base;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectCharredGround2Pragmium : ProtoObjectCharredGround
    {
        public override bool IsRemovesOtherCharredGroundInOccupiedTiles => false;

        [NotLocalizable]
        public override string Name => "Charred ground (Pragmium)";

        // despawn after 30 minutes (same as pragmium nodes despawn time)
        public override double ObjectDespawnDurationSeconds { get; } = 30 * 60;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);

            renderer.PositionOffset += (0, 0.1);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("####",
                         "####",
                         "####");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource("FX/Explosions/ExplosionGround2Pragmium",
                                       isTransparent: true);
        }
    }
}