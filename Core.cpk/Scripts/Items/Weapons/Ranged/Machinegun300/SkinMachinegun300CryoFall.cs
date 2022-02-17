namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinMachinegun300CryoFall : ItemMachinegun300
    {
        public override string Name => SkinName.CryoFall;

        protected override Vector2D? MuzzleFlashTextureOffset => (296, 82);
    }
}