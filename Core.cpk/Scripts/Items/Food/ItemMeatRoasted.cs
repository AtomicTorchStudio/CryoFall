namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemMeatRoasted : ProtoItemFood
    {
        public override string Description =>
            "Roasted meat served with some parsley. Where does the parsley come from? It's a mystery!";

        public override float FoodRestore => 15;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Roasted meat";

        public override ushort OrganicValue => 10;
    }
}