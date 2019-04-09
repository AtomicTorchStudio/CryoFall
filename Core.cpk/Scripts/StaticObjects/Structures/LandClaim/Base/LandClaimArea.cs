namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class LandClaimArea
        : ProtoGameObject<ILogicObject, LandClaimAreaPrivateState, LandClaimAreaPublicState, EmptyClientState>,
          IProtoLogicObject
    {
        public override double ClientUpdateIntervalSeconds => double.MaxValue; // never

        public override string Name => "Land claim area";

        public override double ServerUpdateIntervalSeconds => double.MaxValue; // never

        public override void ServerOnDestroy(ILogicObject gameObject)
        {
            base.ServerOnDestroy(gameObject);
            LandClaimSystem.ServerUnregisterArea(gameObject);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            LandClaimSystem.ServerRegisterArea(data.GameObject);
        }
    }
}