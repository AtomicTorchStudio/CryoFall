namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinToxinProliferatorHive : ItemToxinProliferator
    {
        public override string Name => SkinName.Hive;

        protected override Vector2D? MuzzleFlashTextureOffset => (247, 83);
    }
}