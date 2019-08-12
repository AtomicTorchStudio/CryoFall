namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;

    public interface IProtoItemToolMining : IProtoItemWeapon, IProtoItemTool
    {
        double DamageToMinerals { get; }
    }
}