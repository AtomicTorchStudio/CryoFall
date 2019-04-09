namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using AtomicTorch.CBND.CoreMod.Technologies;

    public class ViewModelTechNodeEffectPerkUnlock : BaseViewModelTechNodeEffect
    {
        private readonly TechNodeEffectPerkUnlock effect;

        public ViewModelTechNodeEffectPerkUnlock(TechNodeEffectPerkUnlock effect) : base(effect)
        {
            this.effect = effect;
        }

        public string Title => this.effect.Perk.Name;
    }
}