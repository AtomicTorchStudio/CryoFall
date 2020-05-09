namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry3;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemCanisterGasoline : ProtoItemCanisterWithLiquid, IProtoItemWithReferenceTech
    {
        public override string Description => "Canister of gasoline fuel.";

        public override LiquidType LiquidType => LiquidType.Gasoline;

        public override string Name => "Gasoline canister";

        public TechNode ReferenceTech => Api.GetProtoEntity<TechNodeOilRefinery>();
    }
}