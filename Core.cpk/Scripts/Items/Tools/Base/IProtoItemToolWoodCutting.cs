namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;

    public interface IProtoItemToolWoodcutting : IProtoItemWeapon, IProtoItemTool
    {
        double DamageToTree { get; }
    }
}