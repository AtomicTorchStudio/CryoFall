using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinFlintlockPistolEmperor : ItemFlintlockPistol
    {
        public override string Name => SkinName.Emperor;

        protected override Vector2D? MuzzleFlashTextureOffset => (186, 48);
    }
}