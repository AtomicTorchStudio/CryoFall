namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    /// <summary>
    /// This is a drone-only tool description.
    /// No instances of this item are created but properties of this class are necessary to determine
    /// the mining range and speed.
    /// </summary>
    public class ItemDroneIndustrialStandardTool : ProtoItemDroneTool
    {
        public override double DamageToMinerals => 25;

        public override double DamageToTree => 20;

        public override double FireInterval => 1 / 3.0;
    }
}