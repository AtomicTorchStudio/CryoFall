namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using AtomicTorch.CBND.CoreMod.Technologies;

    public class ViewModelTechNodeEffectVehicleUnlock : BaseViewModelTechNodeEffect
    {
        private readonly TechNodeEffectVehicleUnlock effect;

        public ViewModelTechNodeEffectVehicleUnlock(TechNodeEffectVehicleUnlock effect) : base(effect)
        {
            this.effect = effect;
        }

        public string Title => this.effect.Vehicle.Name;
    }
}