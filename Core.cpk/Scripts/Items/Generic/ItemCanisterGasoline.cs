namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;

    public class ItemCanisterGasoline : ProtoItemCanisterWithLiquid
    {
        public override string Description => "Canister of gasoline fuel.";

        public override LiquidType LiquidType => LiquidType.Gasoline;

        public override string Name => "Gasoline canister";
    }
}