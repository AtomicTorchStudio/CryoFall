namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Ruins.Gates
{
    public sealed class ObjectGateRuinsHorizontal : ObjectGateRuins
    {
        public override double HorizontalDoorBorderWidth => 0.375;

        protected override double DrawWorldOffsetYHorizontalDoor => 0;

        protected override bool IsHorizontalGate => true;
    }
}