namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoObjectMisc : ProtoStaticWorldObject
    {
        public virtual bool CanFlipSprite => true;

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public override double ObstacleBlockDamageCoef => 1;

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override float StructurePointsMax => 9001; // it's non-damageable anyway

        public sealed override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
            damageApplied = 0; // no damage
            return true;       // hit
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            // don't use base implementation
            //base.ClientInitialize(data);

            var renderer = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                textureResource: this.DefaultTexture);
            data.ClientState.Renderer = renderer;

            // flip renderer with some deterministic randomization
            if (this.CanFlipSprite
                && PositionalRandom.Get(data.GameObject.TilePosition, 0, 3, seed: 721886451) == 0)
            {
                renderer.DrawMode = DrawMode.FlipHorizontally;
            }

            this.ClientSetupRenderer(renderer);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var folderPath = SharedGetRelativeFolderPath(thisType, typeof(ProtoObjectMisc));
            return new TextureResource($"StaticObjects/Misc/{folderPath}/{thisType.Name}.png");
        }
    }
}