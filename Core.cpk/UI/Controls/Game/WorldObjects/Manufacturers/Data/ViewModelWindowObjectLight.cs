namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowObjectLight : BaseViewModel
    {
        public const string NotificationNoFuel_Message = "No fuel";

        public const string NotificationNoFuel_Title = "Cannot turn on";

        private readonly ObjectLightPrivateState privateState;

        private readonly IProtoObjectLight protoObjectLight;

        private readonly ObjectLightPublicState publicState;

        private readonly IStaticWorldObject worldObject;

        private ObjectLightMode selectedLightMode;

        public ViewModelWindowObjectLight()
        {
        }

        public ViewModelWindowObjectLight(
            IStaticWorldObject worldObject,
            ObjectLightPrivateState privateState,
            ObjectLightPublicState publicState)
        {
            this.worldObject = worldObject;
            this.protoObjectLight = (IProtoObjectLight)worldObject.ProtoStaticWorldObject;
            this.FuelCapacity = this.protoObjectLight.FuelCapacity;

            this.privateState = privateState;
            this.publicState = publicState;
            this.ContainerInput = (IClientItemsContainer)privateState.ContainerInput;

            var character = ClientCurrentCharacterHelper.Character;
            ClientContainersExchangeManager.Register(this,
                                                     this.ContainerInput,
                                                     allowedTargets: new[]
                                                     {
                                                         character.SharedGetPlayerContainerInventory(),
                                                         character.SharedGetPlayerContainerHotbar()
                                                     });

            var (icon, color) = this.protoObjectLight.FuelItemsContainerPrototype.ClientGetFuelIconAndColor();
            this.FuelIcon = Client.UI.GetTextureBrush(icon);
            this.FuelColor = color;

            this.privateState.ClientSubscribe(
                _ => _.FuelAmount,
                _ => this.RefreshFuelAmount(),
                this);

            this.privateState.ClientSubscribe(
                _ => _.Mode,
                _ => this.RefreshMode(),
                this);

            publicState.ClientSubscribe(
                _ => _.IsLightActive,
                _ => this.RefreshIsLightActive(),
                this);

            this.RefreshFuelAmount();
            this.RefreshMode();
            this.RefreshIsLightActive();
        }

        public IClientItemsContainer ContainerInput { get; }

        public double FuelAmount { get; set; }

        public double FuelCapacity { get; }

        public Color FuelColor { get; }

        public Brush FuelIcon { get; }

        public bool IsLightActive { get; set; }

        public bool LightModeIsAuto
        {
            get => this.selectedLightMode == ObjectLightMode.Auto;
            set => this.SetLightMode(ObjectLightMode.Auto);
        }

        public bool LightModeIsOff
        {
            get => this.selectedLightMode == ObjectLightMode.Off;
            set => this.SetLightMode(ObjectLightMode.Off);
        }

        public bool LightModeIsOn
        {
            get => this.selectedLightMode == ObjectLightMode.On;
            set => this.SetLightMode(ObjectLightMode.On);
        }

        private void RefreshFuelAmount()
        {
            this.FuelAmount = this.privateState.FuelAmount;
        }

        private void RefreshIsLightActive()
        {
            this.IsLightActive = this.publicState.IsLightActive;
        }

        private void RefreshMode()
        {
            this.selectedLightMode = this.privateState.Mode;
        }

        private void SetLightMode(ObjectLightMode mode)
        {
            if (this.selectedLightMode == mode)
            {
                return;
            }

            if (mode == ObjectLightMode.On
                && this.FuelAmount <= 0)
            {
                NotificationSystem.ClientShowNotification(
                    NotificationNoFuel_Title,
                    NotificationNoFuel_Message,
                    NotificationColor.Bad,
                    this.protoObjectLight.Icon);
                return;
            }

            this.selectedLightMode = mode;
            this.protoObjectLight.ClientSetServerMode(this.worldObject, mode);

            this.NotifyPropertyChanged(nameof(this.LightModeIsOn));
            this.NotifyPropertyChanged(nameof(this.LightModeIsOff));
            this.NotifyPropertyChanged(nameof(this.LightModeIsAuto));
        }
    }
}