namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage
{
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectPowerGridPrivateState : StructurePrivateState
    {
        [SyncToClient]
        public ILogicObject PowerGrid { get; set; }
    }
}