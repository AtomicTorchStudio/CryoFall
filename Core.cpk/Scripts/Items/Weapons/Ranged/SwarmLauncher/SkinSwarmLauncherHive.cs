namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinSwarmLauncherHive : ItemSwarmLauncher
    {
        public override string Name => SkinName.Hive;

        protected override Vector2D? MuzzleFlashTextureOffset => (245, 67);
    }
}