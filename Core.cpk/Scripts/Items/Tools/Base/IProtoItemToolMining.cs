namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoItemToolMining : IProtoItemWeapon, IProtoItemTool
    {
        double DamageToMinerals { get; }

        double ServerGetDamageToMineral(IStaticWorldObject targetObject);
    }
}