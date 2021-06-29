namespace AtomicTorch.CBND.CoreMod.Items.Reactor
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemReactorFuelRod : ProtoItemWithDurability
    {
        public override string Description =>
            "Pragmium-filled reactor fuel rod. Relatively stable, portable and long-lasting power source.";

        public sealed override uint DurabilityMax => uint.MaxValue;

        public sealed override bool IsRepairable => false;

        public double LifetimeDuration => 3 * 24 * 60 * 60;

        public override string Name => "Pragmium fuel rod";

        public double OutputElectricityPerSecond => 10;

        public double PsiEmissionLevel => 1.2;

        public override void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId)
        {
            // place an empty reactor core to the released container slot
            Server.Items.CreateItem<ItemReactorFuelRodEmpty>(container, slotId: slotId);
        }

        protected override string GenerateIconPath()
        {
            return "Items/Reactor/" + this.GetType().Name;
        }
    }
}