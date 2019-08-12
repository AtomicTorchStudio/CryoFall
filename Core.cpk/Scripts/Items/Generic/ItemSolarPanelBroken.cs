namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.GameApi.Resources;

    public class ItemSolarPanelBroken : ProtoItemGeneric
    {
        public static readonly ITextureResource ObjectSpriteBroken
            = new TextureResource("StaticObjects/Structures/Generators/SolarPanels/ObjectSolarPanelBroken");

        public override string Description => "Broken panels can be recycled to produce new solar panels.";

        public override double GroundIconScale => 2;

        public override ushort MaxItemsPerStack => ItemStackSize.Single;

        public override string Name => "Broken solar panel";
    }
}