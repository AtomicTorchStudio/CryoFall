namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ClientEquipmentHeadWithLightInputToggle
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
                                                () =>
                                                {
                                                    var item = ClientCurrentCharacterHelper
                                                               .PublicState?
                                                               .ContainerEquipment
                                                               .Items
                                                               .FirstOrDefault(
                                                                   i => i.ProtoItem is
                                                                            IProtoItemEquipmentHeadWithLight);

                                                    if (item == null)
                                                    {
                                                        NotificationSystem.ClientShowNotification(
                                                            NotificationNoHelmetLightEquipped,
                                                            color: NotificationColor.Bad);
                                                        return;
                                                    }

                                                    var protoLight = (IProtoItemEquipmentHeadWithLight)item.ProtoItem;
                                                    protoLight.ClientToggleLight(item);
                                                });
        }
    }
}