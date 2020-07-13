namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDroneControl
{
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class CharacterDroneController
        : ProtoGameObject<ILogicObject,
              EmptyPrivateState,
              CharacterDroneController.PublicState,
              EmptyClientState>,
          IProtoLogicObject
    {
        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public override string Name => nameof(CharacterDroneController);

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            data.PublicState.Init();
        }

        public class PublicState : BasePublicState
        {
            [SyncToClient]
            [TempOnly]
            public NetworkSyncList<IDynamicWorldObject> CurrentlyControlledDrones { get; private set; }

            public void Init()
            {
                if (this.CurrentlyControlledDrones is null)
                {
                    this.CurrentlyControlledDrones = new NetworkSyncList<IDynamicWorldObject>();
                }
            }
        }
    }
}