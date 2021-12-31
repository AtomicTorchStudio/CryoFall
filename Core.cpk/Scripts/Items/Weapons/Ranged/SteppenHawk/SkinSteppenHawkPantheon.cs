using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinSteppenHawkPantheon : ItemSteppenHawk
    {
        public override string Name => SkinName.Pantheon;

        protected override Vector2D? MuzzleFlashTextureOffset => (176, 75);
    }
}