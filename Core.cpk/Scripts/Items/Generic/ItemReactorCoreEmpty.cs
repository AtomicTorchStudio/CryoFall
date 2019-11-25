namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemReactorCoreEmpty : ProtoItemGeneric
    {
        public override string Description =>
            "Empty reactor core that can be refilled with reactive energy carrier substance such as pragmium.";

        public override ushort MaxItemsPerStack => ItemStackSize.Single;

        public override string Name => "Empty reactor core";
    }
}