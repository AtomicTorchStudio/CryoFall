using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinHandgun10mmUrbanCamo : ItemHandgun10mm
    {
        public override string Name => SkinName.UrbanCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (134, 81);
    }
}