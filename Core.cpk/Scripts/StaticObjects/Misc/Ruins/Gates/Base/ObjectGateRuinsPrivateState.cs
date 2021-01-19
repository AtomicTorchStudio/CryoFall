namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Ruins.Gates
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectGateRuinsPrivateState : ObjectDoorPrivateState
    {
        [TempOnly]
        public double OpenedUntil { get; set; }
    }
}