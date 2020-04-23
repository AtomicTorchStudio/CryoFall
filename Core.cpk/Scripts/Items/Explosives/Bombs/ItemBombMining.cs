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

        // it will result in 10 XP as normally bomb provides 250 XP
        protected override double PlantingExperienceMultiplier => 1 / 25.0;

        protected override void PrepareProtoItemExplosive(
            out IProtoObjectExplosive objectExplosiveProto)
        {
            objectExplosiveProto = GetProtoEntity<ObjectBombMining>();
        }
    }
}