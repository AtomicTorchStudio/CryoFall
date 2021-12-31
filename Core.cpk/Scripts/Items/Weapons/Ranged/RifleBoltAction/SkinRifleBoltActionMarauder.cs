using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinRifleBoltActionMarauder : ItemRifleBoltAction
    {
        public override string Name => SkinName.Marauder;

        protected override Vector2D? MuzzleFlashTextureOffset => (333, 62);
    }
}