namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectGeneratorPragmiumPublicState : ObjectGeneratorPublicState
    {
        [SyncToClient]
        [TempOnly]
        public ObjectGeneratorPragmiumReactorPublicState[] ReactorStates { get; set; }
    }
}