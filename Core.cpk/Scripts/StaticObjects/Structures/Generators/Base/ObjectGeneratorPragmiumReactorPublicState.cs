namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ObjectGeneratorPragmiumReactorPublicState : BaseNetObject
    {
        [TempOnly]
        [SyncToClient(DeliveryMode.ReliableSequenced, networkDataType: typeof(byte))]
        public byte ActivationProgressPercents { get; set; }
    }
}