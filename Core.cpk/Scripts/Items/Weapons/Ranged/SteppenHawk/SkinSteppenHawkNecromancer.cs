using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinSteppenHawkNecromancer : ItemSteppenHawk
    {
        public override string Name => SkinName.Necromancer;

        protected override Vector2D? MuzzleFlashTextureOffset => (160, 82);
    }
}