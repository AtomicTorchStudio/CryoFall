using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinRifle10mmUrbanCamo : ItemRifle10mm
    {
        public override string Name => SkinName.UrbanCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (317, 59);
    }
}