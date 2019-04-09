namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;

    public class ItemCanisterMineralOil : ProtoItemCanisterWithLiquid
    {
        public override string Description => "Canister with mineral oil.";

        public override LiquidType LiquidType => LiquidType.MineralOil;

        public override string Name => "Mineral oil canister";
    }
}