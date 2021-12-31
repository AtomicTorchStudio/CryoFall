using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinRevolver8mmPantheon : ItemRevolver8mm
    {
        public override string Name => SkinName.Pantheon;

        protected override Vector2D? MuzzleFlashTextureOffset => (164, 65);
    }
}