namespace AtomicTorch.CBND.CoreMod.Items.Reactor
{
    public class ItemReactorBrokenModule : ProtoItemGeneric
    {
        public override string Description =>
            "Scrap leftover after one of the reactor modules has broken down.";

        public override string Name => "Broken reactor module";

        protected override string GenerateIconPath()
        {
            return "Items/Reactor/" + this.GetType().Name;
        }
    }
}