namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.GameApi.Resources;

    public class ItemSolarPanel : ProtoItemSolarPanel
    {
        public override string Description =>
            "Expensive to produce, but it will provide electricity for an extremely long time without any maintenance.";

        public override ushort DurabilityDecreasePerMinuteWhenInstalled => 1;

        // It's enough for 3 real time days, however it's decreased only half the time since
        // the solar power is not generated at nights (and reduced during morning/evening).
        // So effectively it's about 5 real time days.
        public override uint DurabilityMax => 3 * 24 * 60;

        public override double ElectricityProductionPerSecond => 1.0;

        public override string Name => "Solar panel";

        public override ITextureResource ObjectSprite { get; }
            = new TextureResource("StaticObjects/Structures/Generators/SolarPanels/ObjectSolarPanel");
    }
}