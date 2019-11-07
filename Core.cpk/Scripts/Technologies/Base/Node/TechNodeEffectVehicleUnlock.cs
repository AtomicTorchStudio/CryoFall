namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TechNodeEffectVehicleUnlock : BaseTechNodeEffect
    {
        public TechNodeEffectVehicleUnlock(IProtoVehicle vehicle)
        {
            this.Vehicle = vehicle ?? throw new ArgumentNullException();
        }

        public override string Description => "Unlocks recipe " + this.Vehicle.Name;

        public override ITextureResource Icon => this.Vehicle.Icon;

        public override string ShortDescription => this.Vehicle.Name;

        public IProtoVehicle Vehicle { get; }

        public override BaseViewModelTechNodeEffect CreateViewModel()
        {
            return new ViewModelTechNodeEffectVehicleUnlock(this);
        }

        public override void PrepareEffect(TechNode techNode)
        {
            this.Vehicle.PrepareProtoSetLinkWithTechNode(techNode);
        }
    }
}