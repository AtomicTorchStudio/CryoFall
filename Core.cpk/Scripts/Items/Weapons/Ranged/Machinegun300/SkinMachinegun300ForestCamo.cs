using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinMachinegun300ForestCamo : ItemMachinegun300
    {
        public override string Name => SkinName.ForestCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (291, 90);
    }
}