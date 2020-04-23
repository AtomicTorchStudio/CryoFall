namespace AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs;

    public class ItemBombResonance : ProtoItemExplosive
    {
        public override TimeSpan DeployDuration => TimeSpan.FromSeconds(6);

        public override string Description =>
            "Special bomb that utilizes unique properties of pragmium to damage multiple layers of walls or doors at once. Helps to breach especially well-defended bases. Does not affect any other structures in any way.";

        public override string Name => "Resonance bomb";

        protected override double PlantingExperienceMultiplier => 2;

        protected override void PrepareProtoItemExplosive(
            out IProtoObjectExplosive objectExplosiveProto)
        {
            objectExplosiveProto = GetProtoEntity<ObjectBombResonance>();
        }
    }
}