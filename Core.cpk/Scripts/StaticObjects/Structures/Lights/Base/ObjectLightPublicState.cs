namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectLightPublicState : StaticObjectPublicState
    {
        [SyncToClient(DeliveryMode.ReliableSequenced)]
        public bool IsLightActive { get; set; }
    }
}