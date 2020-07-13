namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowSprinkler : BaseViewModel
    {
        private readonly ProtoObjectSprinkler.PrivateState privateState;

        private readonly IProtoObjectSprinkler protoSprinkler;

        private readonly IStaticWorldObject worldObject;

        public ViewModelWindowSprinkler(
            IStaticWorldObject worldObject,
            ProtoObjectSprinkler.PrivateState privateState,
            ManufacturingConfig manufacturingConfig)
        {
            this.protoSprinkler = (IProtoObjectSprinkler)worldObject.ProtoStaticWorldObject;
            this.worldObject = worldObject;
            this.privateState = privateState;

            this.ViewModelManufacturingState = new ViewModelManufacturingState(
                worldObject,
                this.privateState.ManufacturingState,
                manufacturingConfig);

            this.privateState.ClientSubscribe(
                _ => _.WaterAmount,
                _ => this.RefreshWaterAmount(),
                this);

            this.privateState.ClientSubscribe(
                _ => _.NextWateringTime,
                _ => this.NotifyPropertyChanged(nameof(this.NextWateringInText)),
                this);

            this.RefreshWaterAmount();

            this.RefreshNextWateringInText();
        }

        public bool CanTryWateringNow
        {
            get
            {
                var time = Client.CurrentGame.ServerFrameTimeApproximated;
                if (this.privateState.LastWateringTime < double.MaxValue
                    && time - this.privateState.LastWateringTime < ProtoObjectSprinkler.WateringCooldownSeconds)
                {
                    // watered recently
                    return false;
                }

                return true;
            }
        }

        public BaseCommand CommandWaterNow
            => new ActionCommand(this.ExecuteCommandWaterNow);

        public uint ElectricityConsumptionPerWatering
            => this.protoSprinkler.ElectricityConsumptionPerWatering;

        public string NextWateringInText
        {
            get
            {
                string text;

                if (this.privateState.NextWateringTime < double.MaxValue)
                {
                    var deltaTime = this.privateState.NextWateringTime
                                    - Client.CurrentGame.ServerFrameTimeApproximated;

                    if (deltaTime <= 1)
                    {
                        deltaTime = 1;
                    }

                    text = ClientTimeFormatHelper.FormatTimeDuration(deltaTime);
                }
                else
                {
                    text = CoreStrings.Item_SpoiledIn_Never;
                }

                return string.Format(CoreStrings.WindowSprinkler_NextWateringIn_Format, text);
            }
        }

        public ViewModelManufacturingState ViewModelManufacturingState { get; }

        public double WaterAmount { get; set; }

        public double WaterCapacity => this.protoSprinkler.WaterCapacity;

        public double WaterConsumptionPerWatering => this.protoSprinkler.WaterConsumptionPerWatering;

        private void ExecuteCommandWaterNow()
        {
            this.protoSprinkler.ClientWaterNow(this.worldObject);
        }

        private void RefreshNextWateringInText()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.NextWateringInText));
            this.NotifyPropertyChanged(nameof(this.CanTryWateringNow));
            ClientTimersSystem.AddAction(delaySeconds: 1,
                                         this.RefreshNextWateringInText);
        }

        private void RefreshWaterAmount()
        {
            this.WaterAmount = this.privateState.WaterAmount;
        }
    }
}