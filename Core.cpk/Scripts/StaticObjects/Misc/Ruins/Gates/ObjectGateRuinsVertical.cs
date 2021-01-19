namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Ruins.Gates
{
    public sealed class ObjectGateRuinsVertical : ObjectGateRuins
    {
        protected override double DrawWorldOffsetYVerticalDoor => 0.015;

        protected override bool IsHorizontalGate => false;

        protected override double VerticalDoorOpenedColliderHeight => 0.375;
    }
}