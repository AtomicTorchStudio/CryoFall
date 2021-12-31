using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinRifle300UrbanCamo : ItemRifle300
    {
        public override string Name => SkinName.UrbanCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (336, 68);
    }
}