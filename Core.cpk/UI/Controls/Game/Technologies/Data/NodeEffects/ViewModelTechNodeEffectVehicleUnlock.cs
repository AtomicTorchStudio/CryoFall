namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelTechNodeEffectVehicleUnlock : BaseViewModelTechNodeEffect
    {
        private readonly TechNodeEffectVehicleUnlock effect;

        public ViewModelTechNodeEffectVehicleUnlock(TechNodeEffectVehicleUnlock effect) : base(effect)
        {
            this.effect = effect;
        }

        public IReadOnlyList<IProtoItem> RequiredProtoItems
        {
            get
            {
                var inputItems = this.effect.Vehicle.BuildRequiredItems;
                var array = new IProtoItem[inputItems.Count];
                for (var index = 0; index < inputItems.Count; index++)
                {
                    array[index] = inputItems[index].ProtoItem;
                }

                return array;
            }
        }

        public string Title => this.effect.Vehicle.Name;
    }
}