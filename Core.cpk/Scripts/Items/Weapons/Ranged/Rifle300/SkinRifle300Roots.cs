using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinRifle300Roots : ItemRifle300
    {
        public override string Name => SkinName.Roots;

        protected override Vector2D? MuzzleFlashTextureOffset => (346, 60);
    }
}