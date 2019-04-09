namespace AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs;

    public class ItemBombModern : ProtoItemExplosive
    {
        public override TimeSpan DeployDuration => TimeSpan.FromSeconds(6);

        public override string Description => "High explosive device designed to breach reinforced structures.";

        public override string Name => "Modern bomb";

        protected override double PlantingExperienceMultiplier => 2;

        protected override void PrepareProtoItemExplosive(
            out IProtoObjectExplosive objectExplosiveProto)
        {
            objectExplosiveProto = GetProtoEntity<ObjectBombModern>();
        }
    }
}