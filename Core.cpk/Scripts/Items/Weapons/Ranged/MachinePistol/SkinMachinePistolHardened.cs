using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinMachinePistolHardened : ItemMachinePistol
    {
        public override string Name => SkinName.Hardened;

        protected override Vector2D? MuzzleFlashTextureOffset => (180, 86);
    }
}