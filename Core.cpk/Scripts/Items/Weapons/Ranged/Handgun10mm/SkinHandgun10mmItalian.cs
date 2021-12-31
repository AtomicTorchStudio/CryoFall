using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinHandgun10mmItalian : ItemHandgun10mm
    {
        public override string Name => SkinName.Italian;

        protected override Vector2D? MuzzleFlashTextureOffset => (137, 90);
    }
}