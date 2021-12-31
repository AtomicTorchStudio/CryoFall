using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinGrenadeLauncherRedMarble : ItemGrenadeLauncher
    {
        public override string Name => SkinName.RedMarble;

        protected override Vector2D? MuzzleFlashTextureOffset => (258, 83);
    }
}