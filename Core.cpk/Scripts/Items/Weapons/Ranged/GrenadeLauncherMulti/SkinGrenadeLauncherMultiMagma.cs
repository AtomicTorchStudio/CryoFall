namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinGrenadeLauncherMultiMagma : ItemGrenadeLauncherMulti
    {
        public override string Name => SkinName.Magma;

        protected override Vector2D? MuzzleFlashTextureOffset => (263, 59);
    }
}