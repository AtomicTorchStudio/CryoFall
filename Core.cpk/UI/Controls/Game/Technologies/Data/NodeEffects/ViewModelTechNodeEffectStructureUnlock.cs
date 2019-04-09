namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using AtomicTorch.CBND.CoreMod.Technologies;

    public class ViewModelTechNodeEffectStructureUnlock : BaseViewModelTechNodeEffect
    {
        private readonly TechNodeEffectStructureUnlock effect;

        public ViewModelTechNodeEffectStructureUnlock(TechNodeEffectStructureUnlock effect) : base(effect)
        {
            this.effect = effect;
        }

        public string Title => this.effect.Structure.Name;
    }
}