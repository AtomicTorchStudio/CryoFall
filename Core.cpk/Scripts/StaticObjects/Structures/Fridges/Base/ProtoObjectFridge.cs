namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class ProtoObjectFridge
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectCrate
            <TPrivateState,
                TPublicState,
                TClientState>
        where TPrivateState : ObjectCratePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        protected override IProtoItemsContainer ItemsContainerType
            => Api.GetProtoEntity<ItemsContainerFridge>();
    }

    public abstract class ProtoObjectFridge
        : ProtoObjectFridge<
            ObjectCratePrivateState,
            StaticObjectPublicState,
            StaticObjectClientState>
    {
    }
}