namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoItemToolWoodcutting : IProtoItemWeapon, IProtoItemTool
    {
        double DamageToTree { get; }

        double ServerGetDamageToTree(IStaticWorldObject targetObject);
    }
}