namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectGeneratorPragmiumPrivateState : ObjectGeneratorPrivateState
    {
        [SyncToClient]
        public ObjectGeneratorPragmiumReactorPrivateState[] ReactorStates { get; set; }
    }
}