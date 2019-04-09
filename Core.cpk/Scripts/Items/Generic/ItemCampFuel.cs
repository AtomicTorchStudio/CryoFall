namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemCampFuel : ProtoItemGeneric, IProtoItemFuelOil
    {
        public override string Description =>
            "Camp fuel (also known as lamp oil) can be used to refill different light sources that burn this type of oil.";

        public double FuelAmount => 250; // about 4 minutes per bottle

        public override string Name => "Camp fuel";
    }
}