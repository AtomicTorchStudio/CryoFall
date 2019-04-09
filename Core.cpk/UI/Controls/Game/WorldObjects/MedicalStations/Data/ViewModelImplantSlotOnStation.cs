namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.MedicalStations.Data
{
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelImplantSlotOnStation : BaseViewModel
    {
        public const string NotificationAlreadyInstalled_Message = "You cannot install two implants of the same type.";

        public const string NotificationAlreadyInstalled_Title = "Already installed";

        public const string NotificationNotEnoughBiomaterial = "Not enough biomaterial";

        private static readonly ItemVialBiomaterial ProtoItemBiomaterialVial
            = Api.GetProtoEntity<ItemVialBiomaterial>();

        private readonly IClientItemsContainer handItemsContainer;

        public ViewModelImplantSlotOnStation(
            IClientItemsContainer containerEquipment,
            byte containerEquipmentSlotId)
        {
            this.ContainerEquipment = containerEquipment;
            this.ContainerEquipmentSlotId = containerEquipmentSlotId;
            this.ContainerEquipment.ItemsReset += this.ContainerEquipmentItemsResetHandler;
            this.ContainerEquipment.ItemAdded += this.ContainerEquipmentItemAddedHandler;
            this.ContainerEquipment.ItemRemoved += this.ContainerEquipmentItemRemovedHandler;

            this.handItemsContainer = ClientItemInHandDisplayManager.HandContainer;
            this.handItemsContainer.StateHashChanged += this.HandContainerStateHashChanged;

            this.Refresh();
        }

        public BaseCommand CommandInstall => new ActionCommand(this.ExecuteCommandInstall);

        public BaseCommand CommandUninstall => new ActionCommand(this.ExecuteCommandUninstall);

        public IClientItemsContainer ContainerEquipment { get; }

        public byte ContainerEquipmentSlotId { get; }

        public ushort Price { get; private set; } = 123;

        public Visibility VisibilityInstallMode { get; private set; }

        public Visibility VisibilityPrice { get; private set; }

        public Visibility VisibilitySelectImplant { get; private set; }

        public Visibility VisibilityUninstallMode { get; private set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.ContainerEquipment.ItemsReset -= this.ContainerEquipmentItemsResetHandler;
            this.ContainerEquipment.ItemAdded -= this.ContainerEquipmentItemAddedHandler;
            this.ContainerEquipment.ItemRemoved -= this.ContainerEquipmentItemRemovedHandler;
            this.handItemsContainer.StateHashChanged -= this.HandContainerStateHashChanged;
        }

        private void ContainerEquipmentItemAddedHandler(IItem item)
        {
            if (item.ContainerSlotId == this.ContainerEquipmentSlotId)
            {
                this.Refresh();
            }
        }

        private void ContainerEquipmentItemRemovedHandler(IItem item, byte slotId)
        {
            if (slotId == this.ContainerEquipmentSlotId)
            {
                this.Refresh();
            }
        }

        private void ContainerEquipmentItemsResetHandler()
        {
            this.Refresh();
        }

        private void ExecuteCommandInstall()
        {
            if (this.ValidateRequirements())
            {
                ObjectMedicalStation.ClientInstall(this.handItemsContainer.Items.Single(),
                                                   this.ContainerEquipmentSlotId);
            }
        }

        private void ExecuteCommandUninstall()
        {
            if (this.ValidateRequirements())
            {
                ObjectMedicalStation.ClientUninstall(this.ContainerEquipmentSlotId);
            }
        }

        private IItem GetInstalledImplantItem()
        {
            var currentImplant = this.ContainerEquipment.GetItemAtSlot(this.ContainerEquipmentSlotId);
            return currentImplant;
        }

        private IProtoItemEquipmentImplant GetProtoItemToInstall()
        {
            var itemToInstallProto = this.handItemsContainer.Items.FirstOrDefault()?.ProtoItem
                                         as IProtoItemEquipmentImplant;
            if (itemToInstallProto is ItemImplantBroken)
            {
                // cannot install broken implant
                return null;
            }

            return itemToInstallProto;
        }

        private void HandContainerStateHashChanged()
        {
            this.Refresh();
        }

        private void Refresh()
        {
            var installedImplant = this.GetInstalledImplantItem();
            if (installedImplant != null)
            {
                this.VisibilityInstallMode = Visibility.Collapsed;
                this.VisibilitySelectImplant = Visibility.Collapsed;
                this.VisibilityPrice = this.VisibilityUninstallMode = Visibility.Visible;
                var protoImplant = (IProtoItemEquipmentImplant)installedImplant.ProtoItem;
                this.Price = protoImplant.BiomaterialAmountRequiredToUninstall;
                return;
            }

            var protoItemToInstall = this.GetProtoItemToInstall();
            var hasItemToInstall = protoItemToInstall != null;
            this.Price = protoItemToInstall?.BiomaterialAmountRequiredToInstall ?? 0;

            this.VisibilityUninstallMode = Visibility.Collapsed;
            this.VisibilityPrice = this.VisibilityInstallMode = hasItemToInstall
                                                                    ? Visibility.Visible
                                                                    : Visibility.Collapsed;

            this.VisibilitySelectImplant = !hasItemToInstall
                                               ? Visibility.Visible
                                               : Visibility.Collapsed;
        }

        private bool ValidateRequirements()
        {
            // ensure player cannot install the same implant prototype as already installed in another slot
            var itemToInstallProto = this.GetProtoItemToInstall();
            if (itemToInstallProto != null)
            {
                foreach (var equippedItem in this.ContainerEquipment.Items)
                {
                    if (equippedItem.ProtoItem == itemToInstallProto)
                    {
                        NotificationSystem.ClientShowNotification(NotificationAlreadyInstalled_Title,
                                                                  NotificationAlreadyInstalled_Message,
                                                                  color: NotificationColor.Bad,
                                                                  icon: itemToInstallProto.Icon);
                        return false;
                    }
                }
            }

            // check biomaterial requirement
            if (!CreativeModeSystem.ClientIsInCreativeMode()
                && !ClientCurrentCharacterHelper.Character.ContainsItemsOfType(
                    ProtoItemBiomaterialVial,
                    this.Price))
            {
                NotificationSystem.ClientShowNotification(NotificationNotEnoughBiomaterial,
                                                          color: NotificationColor.Bad,
                                                          icon: ProtoItemBiomaterialVial.Icon);
                return false;
            }

            return true;
        }
    }
}