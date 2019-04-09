namespace AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs;

    public class ItemBombPrimitive : ProtoItemExplosive
    {
        public override TimeSpan DeployDuration => TimeSpan.FromSeconds(6);

        public override string Description =>
            "Primitive explosive charge that could be used to breach walls and other fortifications. Must be activated as close to the target as possible for maximum effect.";

        public override string Name => "Primitive bomb";

        protected override double PlantingExperienceMultiplier => 1;

        protected override void PrepareProtoItemExplosive(
            out IProtoObjectExplosive objectExplosiveProto)
        {
            objectExplosiveProto = GetProtoEntity<ObjectBombPrimitive>();
        }
    }
}