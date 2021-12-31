using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinRevolver8mmNoble : ItemRevolver8mm
    {
        public override string Name => SkinName.Noble;

        protected override Vector2D? MuzzleFlashTextureOffset => (166, 74);
    }
}