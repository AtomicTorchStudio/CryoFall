namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemCanisterMineralOil : ProtoItemCanisterWithLiquid, IProtoItemWithReferenceTech
    {
        public override string Description => "Canister with mineral oil.";

        public override LiquidType LiquidType => LiquidType.MineralOil;

        public override string Name => "Mineral oil canister";

        public TechNode ReferenceTech => Api.GetProtoEntity<TechNodeOilRefinery>();
    }
}