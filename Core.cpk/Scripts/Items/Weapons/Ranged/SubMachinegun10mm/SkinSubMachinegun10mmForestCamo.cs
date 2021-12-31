using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinSubMachinegun10mmForestCamo : ItemSubMachinegun10mm
    {
        public override string Name => SkinName.ForestCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (209, 86);
    }
}