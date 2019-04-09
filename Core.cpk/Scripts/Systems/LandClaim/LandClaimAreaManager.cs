namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    /// <summary>
    /// This is a manager class for all land claim areas.
    /// It stores all the land claim zones in <see cref="PublicState" />.
    /// It should exist in a single instance stored in server database.
    /// It is managed exclusively by <see cref="LandClaimSystem" />.
    /// </summary>
    internal class LandClaimAreaManager
        : ProtoGameObject<
              ILogicObject,
              LandClaimAreaManager.PrivateState,
              LandClaimAreaManager.PublicState,
              EmptyClientState>,
          IProtoLogicObject
    {
        public override double ClientUpdateIntervalSeconds => 0;

        public override string Name => nameof(LandClaimAreaManager);

        public override double ServerUpdateIntervalSeconds => 0;

        public class PrivateState : BasePrivateState
        {
        }

        public class PublicState : BasePublicState
        {
            [SyncToClient]
            public NetworkSyncList<ILogicObject> LandClaimAreas { get; set; }
        }
    }
}