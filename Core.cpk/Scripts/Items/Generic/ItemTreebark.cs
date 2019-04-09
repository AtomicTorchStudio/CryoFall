namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemTreebark : ProtoItemGeneric, IProtoItemFuelSolid
    {
        public override string Description => "Bark from some trees. Can be burned in a fire or used for other purposes.";

        public double FuelAmount => 10;

        public override string Name => "Tree bark";
    }
}