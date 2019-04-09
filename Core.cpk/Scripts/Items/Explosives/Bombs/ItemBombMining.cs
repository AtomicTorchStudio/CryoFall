namespace AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs;

    public class ItemBombMining : ProtoItemExplosive
    {
        public override TimeSpan DeployDuration => TimeSpan.FromSeconds(1.5);

        public override string Description =>
            "Special explosive device designed to excavate large quantity of minerals in one go. Must be installed next to a mineral node to be excavated.";

        public override string Name => "Mining charge";

        protected override double PlantingExperienceMultiplier =>
            0; // no heavy weapon experience, since it is not even a weapon

        protected override void PrepareProtoItemExplosive(
            out IProtoObjectExplosive objectExplosiveProto)
        {
            objectExplosiveProto = GetProtoEntity<ObjectBombMining>();
        }
    }
}