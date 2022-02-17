namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinRifle300Roots : ItemRifle300
    {
        public override string Name => SkinName.Roots;

        protected override Vector2D? MuzzleFlashTextureOffset => (346, 60);
    }
}