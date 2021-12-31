using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinGrenadeLauncherMultiForestCamo : ItemGrenadeLauncherMulti
    {
        public override string Name => SkinName.ForestCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (261, 55);
    }
}