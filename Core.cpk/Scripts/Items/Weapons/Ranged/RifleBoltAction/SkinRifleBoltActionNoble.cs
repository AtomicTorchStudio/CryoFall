namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinRifleBoltActionNoble : ItemRifleBoltAction
    {
        public override string Name => SkinName.Noble;

        protected override Vector2D? MuzzleFlashTextureOffset => (333, 63);
    }
}