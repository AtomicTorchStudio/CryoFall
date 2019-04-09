namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;

    public interface IProtoItemToolMining : IProtoItemWeapon
    {
        double DamageToMinerals { get; }
    }
}