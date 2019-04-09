namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    public abstract class ProtoObjectDisplayCase
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectCrate
            <TPrivateState,
                TPublicState,
                TClientState>
        where TPrivateState : ObjectCratePrivateState, new()
        where TPublicState : ObjectDisplayCasePublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public override byte ItemsSlotsCount => 1;

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            data.PublicState.ItemsContainer = data.PrivateState.ItemsContainer;
        }
    }

    public abstract class ProtoObjectDisplayCase
        : ProtoObjectDisplayCase<
            ObjectCratePrivateState,
            ObjectDisplayCasePublicState,
            StaticObjectClientState>
    {
    }
}