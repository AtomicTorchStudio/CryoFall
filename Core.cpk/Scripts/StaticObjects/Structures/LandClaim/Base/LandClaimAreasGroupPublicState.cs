namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class LandClaimAreasGroupPublicState : BasePublicState
    {
        [SyncToClient]
        [TempOnly]
        public double? LastRaidTime { get; set; }
    }
}