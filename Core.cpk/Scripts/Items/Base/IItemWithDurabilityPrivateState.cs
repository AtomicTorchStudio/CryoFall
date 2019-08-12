namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public interface IItemWithDurabilityPrivateState : IPrivateState
    {
        /// <summary>
        /// Current value of durability. Should never exceed the according item durability.
        /// </summary>
        uint DurabilityCurrent { get; set; }
    }
}