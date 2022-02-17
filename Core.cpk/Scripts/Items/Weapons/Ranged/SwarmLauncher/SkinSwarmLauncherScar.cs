namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinSwarmLauncherScar : ItemSwarmLauncher
    {
        public override string Name => SkinName.Scar;

        protected override Vector2D? MuzzleFlashTextureOffset => (260, 66);
    }
}