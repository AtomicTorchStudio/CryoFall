namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectLaunchpadPublicState : StaticObjectPublicState
    {
        [SyncToClient]
        public string LaunchedByPlayerName { get; set; }

        [SyncToClient]
        public double LaunchServerFrameTime { get; set; }
    }
}