namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    // This item is no longer used after to introduction of fuel cells, but we're planning to restore it.
    public class ItemReactorCoreEmpty : ProtoItemGeneric
    {
        public override string Description =>
            "Empty reactor core that can be refilled with reactive energy carrier substance such as pragmium.";

        public override ushort MaxItemsPerStack => ItemStackSize.Single;

        public override string Name => "Empty reactor core";
    }
}