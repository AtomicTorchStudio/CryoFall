namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    public interface IProtoItemWeaponEnergy : IProtoItemWeapon
    {
        double EnergyUsePerShot { get; }
    }
}