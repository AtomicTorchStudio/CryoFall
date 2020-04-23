namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectCratePublicState : StaticObjectPublicState
    {
        [SyncToClient]
        public IProtoEntity IconSource { get; set; }
    }
}