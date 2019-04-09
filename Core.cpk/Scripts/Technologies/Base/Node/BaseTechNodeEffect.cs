namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class BaseTechNodeEffect
    {
        public abstract string Description { get; }

        public abstract ITextureResource Icon { get; }

        public abstract string ShortDescription { get; }

        public abstract BaseViewModelTechNodeEffect CreateViewModel();

        public abstract void PrepareEffect(TechNode techNode);
    }
}