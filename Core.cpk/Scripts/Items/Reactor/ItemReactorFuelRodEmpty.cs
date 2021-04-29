namespace AtomicTorch.CBND.CoreMod.Items.Reactor
{
    public class ItemReactorFuelRodEmpty : ProtoItemGeneric
    {
        public override string Description =>
            "Empty reactor fuel rod that can be refilled with pragmium.";

        public override ushort MaxItemsPerStack => 1;

        public override string Name => "Empty fuel rod";

        protected override string GenerateIconPath()
        {
            return "Items/Reactor/" + this.GetType().Name;
        }
    }
}