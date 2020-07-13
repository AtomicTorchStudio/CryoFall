namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelHUDVehicleEnergyIndicator : BaseViewModel
    {
        private readonly PlayerCharacterPublicState playerCharacterPublicState;

        public ViewModelHUDVehicleEnergyIndicator()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.playerCharacterPublicState = ClientCurrentCharacterHelper.PublicState;
            this.playerCharacterPublicState.ClientSubscribe(_ => _.CurrentVehicle,
                                                            _ => this.Refresh(),
                                                            this);

            this.Refresh();
        }

        public Brush Icon
            => Api.Client.UI.GetTextureBrush(
                Api.GetProtoEntity<ItemFuelCellEmpty>().Icon);

        public Visibility IndicatorVisibility { get; private set; }

        public string TooltipFormat
        {
            get
            {
                var vehicleProto = this.playerCharacterPublicState?.CurrentVehicle?.ProtoGameObject;
                if (vehicleProto is null)
                {
                    return string.Empty;
                }

                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                return $"[b]{vehicleProto.Name}[/b][br]{CoreStrings.Vehicle_Hotbar_EnergyPercentFormat}";
            }
        }

        public ViewModelVehicleEnergy ViewModelVehicleEnergy { get; private set; }

        private void Refresh()
        {
            this.ViewModelVehicleEnergy?.Dispose();

            var vehicle = this.playerCharacterPublicState.CurrentVehicle;
            if (vehicle == null)
            {
                this.IndicatorVisibility = Visibility.Collapsed;
                this.ViewModelVehicleEnergy = null;
                return;
            }

            if (!vehicle.IsInitialized)
            {
                // refresh when vehicle will be initialized
                ClientTimersSystem.AddAction(0.1, this.Refresh);

                this.IndicatorVisibility = Visibility.Collapsed;
                this.ViewModelVehicleEnergy = null;
                return;
            }

            this.ViewModelVehicleEnergy = new ViewModelVehicleEnergy(vehicle);
            this.IndicatorVisibility = Visibility.Visible;
            this.NotifyPropertyChanged(nameof(this.TooltipFormat));
        }
    }
}