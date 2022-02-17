namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinMachinePistolLumen : ItemMachinePistol
    {
        public override string Name => SkinName.Lumen;

        protected override Vector2D? MuzzleFlashTextureOffset => (165, 110);
    }
}