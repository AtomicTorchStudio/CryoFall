namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.GameApi.Resources;

    // This item is no longer used after to introduction of fuel cells, but we're planning to restore it.
    public class ItemReactorCoreEmpty : ProtoItemGeneric
    {
        public override string Description =>
            "Empty reactor core that can be refilled with reactive energy carrier substance such as pragmium.";

        // ensure the object is not visible in trading station
        public override ITextureResource Icon => null;

        public override ushort MaxItemsPerStack => ItemStackSize.Single;

        public override string Name => "Empty reactor core";
    }
}