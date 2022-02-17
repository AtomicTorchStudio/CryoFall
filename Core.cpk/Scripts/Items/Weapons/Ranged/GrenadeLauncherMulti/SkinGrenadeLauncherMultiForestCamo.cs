namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinGrenadeLauncherMultiForestCamo : ItemGrenadeLauncherMulti
    {
        public override string Name => SkinName.ForestCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (261, 55);
    }
}