namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinShotgunMilitaryPhoenix : ItemShotgunMilitary
    {
        public override string Name => SkinName.Phoenix;

        protected override Vector2D? MuzzleFlashTextureOffset => (248, 63);
    }
}