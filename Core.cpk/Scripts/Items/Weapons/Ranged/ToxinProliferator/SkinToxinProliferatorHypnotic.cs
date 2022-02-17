namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinToxinProliferatorHypnotic : ItemToxinProliferator
    {
        public override string Name => SkinName.Hypnotic;

        protected override Vector2D? MuzzleFlashTextureOffset => (280, 89);
    }
}