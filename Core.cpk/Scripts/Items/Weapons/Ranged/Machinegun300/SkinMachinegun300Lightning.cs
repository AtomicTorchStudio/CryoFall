namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinMachinegun300Lightning : ItemMachinegun300
    {
        public override string Name => SkinName.Lightning;

        protected override Vector2D? MuzzleFlashTextureOffset => (273, 78);
    }
}