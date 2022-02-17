namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinMachinePistolHardened : ItemMachinePistol
    {
        public override string Name => SkinName.Hardened;

        protected override Vector2D? MuzzleFlashTextureOffset => (180, 86);
    }
}