namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientEquipmentHeadWithLightInputToggle
    {
        public const string NotificationNoHelmetLightEquipped = "No helmet light equipped";

        private static bool isInitialized;

        public static void Init()
        {
            if (isInitialized)
            {
                return;
            }

            Api.ValidateIsClient();
            isInitialized = true;

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp

            ClientInputContext.Start("Head equipment light toggle")
                              .HandleButtonDown(GameButton.HeadEquipmentLightToggle,
                                                ToggleHelmetLight);
        }

        private static void ToggleHelmetLight()
        {
            var characterPublicState = ClientCurrentCharacterHelper.PublicState;

            // check whether a vehicle is overriding the helmet light switch
            var currentVehicle = characterPublicState.CurrentVehicle;
            if (currentVehicle != null)
            {
                var currentVehicleProto = (IProtoVehicle)currentVehicle.ProtoGameObject;
                if (!currentVehicleProto.IsPlayersHotbarAndEquipmentItemsAllowed)
                {
                    // send light switch to proto vehicle
                    currentVehicleProto.ClientToggleLight();
                    return;
                }
            }

            var item = characterPublicState?.ContainerEquipment.Items.FirstOrDefault(
                i => i.ProtoItem is IProtoItemEquipmentHeadWithLight);

            if (item == null)
            {
                NotificationSystem.ClientShowNotification(NotificationNoHelmetLightEquipped,
                                                          color: NotificationColor.Bad);
                return;
            }

            var protoLight = (IProtoItemEquipmentHeadWithLight)item.ProtoItem;
            protoLight.ClientToggleLight(item);
        }
    }
}