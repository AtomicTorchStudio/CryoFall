namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;

    public class ItemCanisterPetroleum : ProtoItemCanisterWithLiquid
    {
        public override string Description =>
            "Canister with raw petroleum oil. The oil can be refined into other components.";

        public override LiquidType LiquidType => LiquidType.Petroleum;

        public override string Name => "Petroleum canister";
    }
}