namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinMusketEmperor : ItemMusket
    {
        public override string Name => SkinName.Emperor;
        
        protected override Vector2D? MuzzleFlashTextureOffset => (340, 78);
    }
}