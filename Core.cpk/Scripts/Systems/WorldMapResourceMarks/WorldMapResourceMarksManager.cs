namespace AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks
{
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    /// <summary>
    /// This is a manager class for all world map resource marks.
    /// It stores all the world mark zones in <see cref="PublicState" />.
    /// It should exist in a single instance stored in server database.
    /// It is managed exclusively by <see cref="WorldMapResourceMarksSystem" />.
    /// </summary>
    public class WorldMapResourceMarksManager
        : ProtoGameObject<
              ILogicObject,
              WorldMapResourceMarksManager.PrivateState,
              WorldMapResourceMarksManager.PublicState,
              EmptyClientState>,
          IProtoLogicObject
    {
        public override double ClientUpdateIntervalSeconds => 0;

        public override string Name => nameof(WorldMapResourceMarksManager);

        public override double ServerUpdateIntervalSeconds => 0;

        public class PrivateState : BasePrivateState
        {
        }

        public class PublicState : BasePublicState
        {
            [SyncToClient]
            [TempOnly]
            public NetworkSyncList<WorldMapResourceMark> Marks { get; set; }
        }
    }
}