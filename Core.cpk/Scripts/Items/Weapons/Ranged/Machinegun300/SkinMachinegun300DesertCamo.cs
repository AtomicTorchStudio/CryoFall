namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinMachinegun300DesertCamo : ItemMachinegun300
    {
        public override string Name => SkinName.DesertCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (288, 77);
    }
}