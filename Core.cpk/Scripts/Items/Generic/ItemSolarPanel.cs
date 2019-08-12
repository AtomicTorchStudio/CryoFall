namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.GameApi.Resources;

    public class ItemSolarPanel : ProtoItemSolarPanel
    {
        public override string Description =>
            "Expensive to produce, but it will provide electricity for an extremely long time without any maintenance.";

        public override ushort DurabilityDecreasePerMinuteWhenInstalled => 1;

        // It's enough for 3 real time days, however only half the time it's decreased
        // as solar power is not generated during the nights (and reduced during morning/evening).
        // So effectively it's about 5 real time days.
        public override uint DurabilityMax => 3 * 24 * 60;

        public override double ElectricityProductionPerSecond => 0.5f;

        public override string Name => "Solar panel";

        public override ITextureResource ObjectSprite { get; }
            = new TextureResource("StaticObjects/Structures/Generators/SolarPanels/ObjectSolarPanel");
    }
}