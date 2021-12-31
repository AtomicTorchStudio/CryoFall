using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinMachinePistolLumen : ItemMachinePistol
    {
        public override string Name => SkinName.Lumen;

        protected override Vector2D? MuzzleFlashTextureOffset => (165, 110);
    }
}