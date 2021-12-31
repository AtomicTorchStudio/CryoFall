using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinGrenadeLauncherMultiMagma : ItemGrenadeLauncherMulti
    {
        public override string Name => SkinName.Magma;

        protected override Vector2D? MuzzleFlashTextureOffset => (263, 59);
    }
}