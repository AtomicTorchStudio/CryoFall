namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemBricks : ProtoItemGeneric
    {
        public override string Description => "Building material for buildings and walls.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Bricks";
    }
}