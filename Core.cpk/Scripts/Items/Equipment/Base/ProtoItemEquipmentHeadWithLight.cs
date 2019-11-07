namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Lights;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.ItemFuelRefill;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Item prototype for head equipment with light.
    /// </summary>
    public abstract class ProtoItemEquipmentHeadWithLight
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemEquipmentHead
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemEquipmentHeadWithLight,
          IProtoItemWithHotbarOverlay
        where TPrivateState : ItemWithFuelPrivateState, new()
        where TPublicState : ItemWithActiveFlagPublicState, new()
        where TClientState : BaseClientState, new()
    {
        public const string NotificationCannotRefillWhileOn_Message = "Turn it off before refilling.";

        public const string NotificationCannotRefillWhileOn_Title = "Cannot refill";

        public const string NotificationOutOfFuel_Message = "Needs refilling.";

        public const string NotificationOutOfFuel_Title = "Out of fuel";

        public const string NotificationPressKeyToActivate =
            "To activate the helmet light, please equip this item and press [{0}] key.";

        public const string TitleButtonNotSet = "button not set";

        private ClientInputContext helperInputListener;

        public override bool CanBeSelectedInVehicle => true; // required in order to enable light toggle on hoverboard

        public IReadOnlyItemFuelConfig ItemFuelConfig { get; private set; }

        public IReadOnlyItemLightConfig ItemLightConfig { get; private set; }

        public override double ServerUpdateIntervalSeconds => 1;

        public bool ClientCanStartRefill(IItem item)
        {
            var publicState = GetPublicState(item);
            if (!publicState.IsActive)
            {
                return true;
            }

            // light is active - try deactivate it
            this.ClientTrySetActiveState(item, setIsActive: false);
            if (!publicState.IsActive)
            {
                return true;
            }

            // still active!
            NotificationSystem.ClientShowNotification(
                NotificationCannotRefillWhileOn_Title,
                NotificationCannotRefillWhileOn_Message,
                color: NotificationColor.Bad,
                icon: this.Icon);
            return false;
        }

        public Control ClientCreateHotbarOverlayControl(IItem item)
        {
            return new HotbarItemWithFuelOverlayControl(item, this.ItemFuelConfig);
        }

        public override void ClientGetHeadSlotSprites(
            IItem item,
            bool isMale,
            SkeletonResource skeletonResource,
            bool isFrontFace,
            out string spriteFront,
            out string spriteBehind)
        {
            this.VerifyGameObject(item);
            var slotAttachments = isMale
                                      ? this.SlotAttachmentsMale
                                      : this.SlotAttachmentsFemale;

            var isActive = GetPublicState(item).IsActive;

            ProtoItemEquipmentHeadHelper.ClientFindDefaultHeadSprites(
                slotAttachments,
                skeletonResource,
                isFrontFace,
                out spriteFront,
                out spriteBehind,
                headEquipmentName: isActive
                                       ? "HeadActiveEquipment"
                                       : "HeadEquipment");

            if (isActive
                && spriteFront == null)
            {
                // no active sprite found - fallback to default head equipment method
                base.ClientGetHeadSlotSprites(item,
                                              isMale,
                                              skeletonResource,
                                              isFrontFace,
                                              out spriteFront,
                                              out spriteBehind);
            }
        }

        public void ClientOnRefilled(IItem item, bool isCurrentHotbarItem)
        {
            if (isCurrentHotbarItem)
            {
                this.ClientTrySetActiveState(item, setIsActive: true);
            }
        }

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            var isActive = GetPublicState(item).IsActive;

            if (!isActive)
            {
                // not active light
                return;
            }

            var sceneObject = character.ClientSceneObject;
            var componentLightSource = this.ClientCreateLightSource(item, character, sceneObject);
            if (componentLightSource == null)
            {
                return;
            }

            var componentLightInSkeleton = sceneObject.AddComponent<ClientComponentLightInSkeleton>();
            componentLightInSkeleton.Setup(skeletonRenderer,
                                           this.ItemLightConfig,
                                           componentLightSource,
                                           "Head");

            skeletonComponents.Add(componentLightSource);
            skeletonComponents.Add(componentLightInSkeleton);
        }

        public void ClientToggleLight(IItem item)
        {
            var publicState = GetPublicState(item);
            var setIsActive = !publicState.IsActive;
            this.ClientTrySetActiveState(item, setIsActive);
        }

        protected virtual BaseClientComponentLightSource ClientCreateLightSource(
            IItem item,
            ICharacter character,
            IClientSceneObject sceneObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                sceneObject,
                this.ItemLightConfig);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var item = data.GameObject;
            var publicState = data.PublicState;
            var privateState = data.PrivateState;

            publicState.ClientSubscribe(
                _ => _.IsActive,
                _ =>
                {
                    // active state changed - invalidate skeleton renderer (so it will be rebuilt)
                    var ownerCharacter = item.Container?.OwnerAsCharacter;
                    ownerCharacter?.ClientInvalidateSkeletonRenderer();

                    if (ownerCharacter != null
                        && !ownerCharacter.IsCurrentClientCharacter)
                    {
                        this.SoundPresetItem.PlaySound(ItemSound.Use, ownerCharacter);
                    }
                },
                subscriptionOwner: data.ClientState);

            privateState.ClientSubscribe(
                _ => _.FuelAmount,
                newFuelAmount =>
                {
                    // fuel amount changed
                    if (newFuelAmount <= 0
                        && publicState.IsActive)
                    {
                        publicState.IsActive = false;
                        Logger.Info($"Fuel depleted, light turns off: {item}");
                        this.ClientShowOutOfFuelNotification();
                    }
                },
                subscriptionOwner: data.ClientState);
        }

        protected override void ClientItemHotbarSelectionChanged(ClientHotbarItemData data)
        {
            if (data.IsSelected)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                this.helperInputListener = ClientInputContext
                                           .Start("Current head light item refill action listener")
                                           .HandleButtonDown(
                                               GameButton.ItemReload,
                                               ItemFuelRefillSystem.Instance.ClientTryStartAction);
            }
            else
            {
                this.helperInputListener?.Stop();
                this.helperInputListener = null;
            }
        }

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            // never play light "use" sound - it's played when the light switched
            return false;
        }

        protected override void ClientItemUseStart(ClientItemData data)
        {
            var item = data.Item;

            var playerCharacter = Client.Characters.CurrentPlayerCharacter;
            if (ItemFuelRefillSystem.Instance.SharedGetCurrentActionState(playerCharacter)
                != null)
            {
                return;
            }

            ClientTryRefill(item);
            if (ItemFuelRefillSystem.Instance.SharedGetCurrentActionState(playerCharacter)
                != null)
            {
                return;
            }

            var inputKey = ClientInputManager.GetKeyForAbstractButton(
                WrappedButton<GameButton>.GetWrappedButton(GameButton.HeadEquipmentLightToggle));

            var key = inputKey != InputKey.None
                          ? inputKey.ToString()
                          : "<" + TitleButtonNotSet + ">";

            NotificationSystem.ClientShowNotification(
                string.Format(NotificationPressKeyToActivate, key),
                icon: this.Icon);
        }

        protected virtual void ClientShowOutOfFuelNotification()
        {
            NotificationSystem.ClientShowNotification(
                NotificationOutOfFuel_Title,
                NotificationOutOfFuel_Message,
                color: NotificationColor.Neutral,
                icon: this.Icon);
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            base.ClientUpdate(data);

            var item = data.GameObject;
            var currentCharacter = ClientCurrentCharacterHelper.Character;
            if (item.Container == null
                || item.Container.OwnerAsCharacter != currentCharacter
                || item.Container != item.Container.OwnerAsCharacter.SharedGetPlayerContainerEquipment())
            {
                // item of another character or not equipped
                return;
            }

            var publicState = data.PublicState;

            // check if item is in a hotbar selected slot, if not - make it not active
            if (!SharedUpdateActiveState(item, publicState))
            {
                return;
            }

            if (!this.SharedCanActivate(currentCharacter, item))
            {
                // deactivate
                publicState.IsActive = false;
                return;
            }

            this.ItemFuelConfig.SharedTryConsumeFuel(item,
                                                     data.PrivateState,
                                                     data.DeltaTime,
                                                     out var isFuelRanOut);
            if (isFuelRanOut)
            {
                publicState.IsActive = false;
            }
        }

        protected sealed override void PrepareProtoItemEquipmentHead()
        {
            var lightConfig = new ItemLightConfig();
            var fuelConfig = new ItemFuelConfig();
            this.PrepareProtoItemEquipmentHeadWithLight(lightConfig, fuelConfig);
            this.ItemLightConfig = lightConfig.ToReadOnly();
            this.ItemFuelConfig = fuelConfig.ToReadOnly();

            if (IsClient)
            {
                ClientEquipmentHeadWithLightInputToggle.Init();
            }
        }

        protected abstract void PrepareProtoItemEquipmentHeadWithLight(
            ItemLightConfig lightConfig,
            ItemFuelConfig fuelConfig);

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var item = data.GameObject;
            var publicState = data.PublicState;

            // check if item is in a hotbar selected slot, if not - make it not active
            if (!SharedUpdateActiveState(item, publicState))
            {
                return;
            }

            var character = item.Container.OwnerAsCharacter;
            if (character != null)
            {
                if (!this.SharedCanActivate(character, item))
                {
                    // deactivate
                    publicState.IsActive = false;
                    return;
                }

                ServerItemUseObserver.NotifyItemUsed(character, item);
            }

            this.ItemFuelConfig.SharedTryConsumeFuel(item, data.PrivateState, data.DeltaTime, out var isFuelRanOut);
            if (isFuelRanOut)
            {
                publicState.IsActive = false;
            }
        }

        protected virtual bool SharedCanActivate(ICharacter character, IItem item)
        {
            var vehicle = PlayerCharacter.GetPublicState(character).CurrentVehicle;
            if (vehicle is null)
            {
                return true;
            }

            var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
            if (this.CanBeSelectedInVehicle
                && protoVehicle.IsPlayersHotbarAndEquipmentItemsAllowed)
            {
                return true;
            }

            if (IsClient
                && protoVehicle.IsPlayersHotbarAndEquipmentItemsAllowed)
            {
                NotificationSystem.ClientShowNotification(this.Name,
                                                          NotificationItemCannotBeUsedInVehicle,
                                                          NotificationColor.Neutral,
                                                          item.ProtoItem.Icon);
            }

            return false;
        }

        private static void ClientTryRefill(IItem item)
        {
            ItemFuelRefillSystem.Instance.SharedStartAction(
                ItemFuelRefillSystem.Instance.ClientTryCreateRequest(
                    Client.Characters.CurrentPlayerCharacter,
                    item));
        }

        // it's shared - but can be executed for the current character on client side
        private static bool SharedUpdateActiveState(IItem item, TPublicState publicState)
        {
            var itemOwnerCharacter = item.Container?.OwnerAsCharacter;
            if (itemOwnerCharacter != null
                && itemOwnerCharacter.ServerIsOnline
                && item.Container == itemOwnerCharacter.SharedGetPlayerContainerEquipment())
            {
                return publicState.IsActive;
            }

            if (!publicState.IsActive)
            {
                return false;
            }

            Logger.Info(item + " is not in an equipment slot or player is offline - make it inactive");
            publicState.IsActive = false;
            return false;
        }

        private void ClientTrySetActiveState(IItem item, bool setIsActive)
        {
            var publicState = GetPublicState(item);
            if (publicState.IsActive == setIsActive)
            {
                return;
            }

            var privateState = GetPrivateState(item);
            if (setIsActive && privateState.FuelAmount <= 0)
            {
                ClientTryRefill(item);
                return;
            }

            if (setIsActive
                && item.Container
                != item.Container.OwnerAsCharacter?
                    .SharedGetPlayerContainerEquipment())
            {
                Logger.Info(item + " cannot activate - not in an equipment slot");
                return;
            }

            if (setIsActive && !this.SharedCanActivate(ClientCurrentCharacterHelper.Character, item))
            {
                setIsActive = false;
            }

            if (publicState.IsActive == setIsActive)
            {
                return;
            }

            publicState.IsActive = setIsActive;
            Logger.Info($"Player switched head equipment light mode: {item}, isActive={setIsActive}");
            this.CallServer(_ => _.ServerRemote_SetMode(item, setIsActive));

            this.SoundPresetItem.PlaySound(ItemSound.Use);
        }

        [RemoteCallSettings(DeliveryMode.Default)]
        private void ServerRemote_SetMode(IItem item, bool setIsActive)
        {
            this.VerifyGameObject(item);

            var character = ServerRemoteContext.Character;
            var characterPublicState = PlayerCharacter.GetPublicState(character);

            var publicState = GetPublicState(item);
            if (publicState.IsActive == setIsActive)
            {
                Logger.Warning($"The same state is already set: isActive={setIsActive} for {item}", character);
                return;
            }

            if (item.Container != characterPublicState.ContainerEquipment)
            {
                throw new Exception("The head equipment light item must be located in the character equipment: "
                                    + item);
            }

            if (setIsActive && !this.SharedCanActivate(character, item))
            {
                setIsActive = false;
            }

            if (setIsActive)
            {
                var privateState = GetPrivateState(item);
                if (privateState.FuelAmount <= 0)
                {
                    Logger.Warning($"Cannot set the active state - no fuel: {item}", character);
                    return;
                }
            }

            publicState.IsActive = setIsActive;
            Logger.Info($"Player switched light mode: {item}, isActive={setIsActive}", character);
        }
    }

    /// <summary>
    /// Item prototype for head equipment with light.
    /// </summary>
    public abstract class ProtoItemEquipmentHeadWithLight
        : ProtoItemEquipmentHeadWithLight
            <ItemWithFuelPrivateState,
                ItemWithActiveFlagPublicState,
                EmptyClientState>
    {
    }
}