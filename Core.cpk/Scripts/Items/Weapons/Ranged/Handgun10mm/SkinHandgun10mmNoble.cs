using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinHandgun10mmNoble : ItemHandgun10mm
    {
        public override string Name => SkinName.Noble;

        protected override Vector2D? MuzzleFlashTextureOffset => (133, 78);
    }
}