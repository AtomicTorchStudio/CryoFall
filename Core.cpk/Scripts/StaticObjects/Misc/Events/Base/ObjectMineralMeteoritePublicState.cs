namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectMineralMeteoritePublicState : StaticObjectPublicState
    {
        [SyncToClient]
        public double CooldownUntilServerTime { get; set; }
    }
}