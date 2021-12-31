using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinRifleBoltActionHunter : ItemRifleBoltAction
    {
        public override string Name => SkinName.Hunter;

        protected override Vector2D? MuzzleFlashTextureOffset => (333, 70);
    }
}