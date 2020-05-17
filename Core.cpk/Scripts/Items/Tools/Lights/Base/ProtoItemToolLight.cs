namespace AtomicTorch.CBND.CoreMod.Items.Tools.Lights
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.ItemFuelRefill;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Base tool item prototype for light items.
    /// </summary>
    public abstract class ProtoItemToolLight
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemTool
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemToolLight,
          IProtoItemWithHotbarOverlay
        where TPrivateState : ItemWithFuelPrivateState, new()
        where TPublicState : ItemWithActiveFlagPublicState, new()
        where TClientState : BaseClientState, new()
    {
        public const string NotificationCannotRefillWhileOn_Message = "Turn it off before refilling.";

        public const string NotificationCannotRefillWhileOn_Title = "Cannot refill";

        public const string NotificationOutOfFuel_Message = "Needs refilling.";

        public const string NotificationOutOfFuel_Title = "Out of fuel";

        private ClientInputContext helperInputListener;

        protected ProtoItemToolLight()
        {
            var name = this.GetType().Name;

            this.CharacterTextureResourceInactive = new TextureResource(
                "Characters/Tools/" + name,
                isProvidesMagentaPixelPosition: true);

            this.CharacterTextureResourceActive = new TextureResource(
                "Characters/Tools/" + name + "Active",
                isProvidesMagentaPixelPosition: true);
        }

        public override bool CanBeSelectedInVehicle => true;

        public IReadOnlyItemFuelConfig ItemFuelConfig { get; private set; }

        public IReadOnlyItemLightConfig ItemLightConfig { get; private set; }

        // slowly reduce durability of the light item
        public override double ServerUpdateIntervalSeconds => 5;

        protected virtual string ActiveLightCharacterAnimationName => "Torch2";

        protected virtual TextureResource CharacterTextureResourceActive { get; }

        protected virtual TextureResource CharacterTextureResourceInactive { get; }

        protected virtual ushort DurabilityDecreasePerSecond => 1;

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

        public void ClientOnRefilled(IItem item, bool isCurrentHotbarItem)
        {
            if (isCurrentHotbarItem)
            {
                this.ClientTrySetActiveState(item, setIsActive: true);
            }
        }

        public virtual void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            protoCharacterSkeleton.ClientSetupItemInHand(
                skeletonRenderer,
                "WeaponMelee",
                this.GetCharacterTextureResource(item, character));

            var isActive = GetPublicState(item).IsActive;
            this.ClientSetupSkeletonAnimation(isActive, item, character, skeletonRenderer, skeletonComponents);

            if (!isActive)
            {
                // not active light
                return;
            }

            var sceneObject = character.ClientSceneObject;
            var componentLightSource = this.ClientCreateLightSource(item, character);
            var componentLightInSkeleton = sceneObject.AddComponent<ClientComponentLightInSkeleton>();
            componentLightInSkeleton.Setup(skeletonRenderer,
                                           this.ItemLightConfig,
                                           componentLightSource,
                                           "Weapon");

            skeletonComponents.Add(componentLightInSkeleton);
            skeletonComponents.Add(componentLightSource);
        }

        public void ClientTrySetActiveState(IItem item, bool setIsActive)
        {
            var publicState = GetPublicState(item);
            if (publicState.IsActive == setIsActive)
            {
                return;
            }

            var privateState = GetPrivateState(item);
            if (setIsActive && privateState.FuelAmount <= 0)
            {
                // try refill
                ItemFuelRefillSystem.Instance.ClientTryStartAction();
                return;
            }

            publicState.IsActive = setIsActive;
            Logger.Info($"Player switched light mode: {item}, isActive={setIsActive}");
            this.CallServer(_ => _.ServerRemote_SetMode(item, setIsActive));

            this.SoundPresetItem.PlaySound(ItemSound.Use);
        }

        protected virtual BaseClientComponentLightSource ClientCreateLightSource(
            IItem item,
            ICharacter character)
        {
            return ClientLighting.CreateLightSourceSpot(
                character.ClientSceneObject,
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
                    if (item == ownerCharacter?.SharedGetPlayerSelectedHotbarItem())
                    {
                        ownerCharacter.ClientInvalidateSkeletonRenderer();
                    }

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
                        && publicState.IsActive
                        // is rechargable
                        && this.ItemFuelConfig.FuelProtoItemsList.Count > 0)
                    {
                        publicState.IsActive = false;
                        Logger.Info($"Fuel depleted, light turns off: {item}");
                        this.ClientShowOutOfFuelNotification();
                    }
                },
                subscriptionOwner: data.ClientState);

            if (publicState.IsActive)
            {
                var ownerCharacter = item.Container?.OwnerAsCharacter;
                if (ownerCharacter != null
                    && !ownerCharacter.IsCurrentClientCharacter)
                {
                    this.SoundPresetItem.PlaySound(ItemSound.Use, ownerCharacter);
                }
            }
        }

        protected override void ClientItemHotbarSelectionChanged(ClientHotbarItemData data)
        {
            var item = data.Item;

            if (data.IsSelected)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                this.helperInputListener = ClientInputContext
                                           .Start("Current light item refill action listener")
                                           .HandleButtonDown(
                                               GameButton.ItemReload,
                                               ItemFuelRefillSystem.Instance.ClientTryStartAction);

                if (!data.IsByPlayer)
                {
                    // it's automatic selection (player just logged in)
                    if (!TimeOfDaySystem.ClientIsInitialized)
                    {
                        Logger.Warning(nameof(TimeOfDaySystem) + " is not initialized - cannot enable a hand lamp");
                        return;
                    }

                    // don't enable the light automatically if it's not night
                    var isLightRequired = TimeOfDaySystem.IsNight;
                    Logger.Info(string.Format("Light auto selection! Time: {0:hh\\:mm}, is light required: {1}",
                                              TimeSpan.FromHours(TimeOfDaySystem.CurrentTimeOfDayHours),
                                              isLightRequired));

                    if (!isLightRequired)
                    {
                        return;
                    }
                }

                if (data.PrivateState.FuelAmount > 0)
                {
                    // have fuel - turn on automatically
                    this.ClientTrySetActiveState(item, setIsActive: true);
                }
                else
                {
                    this.ClientShowOutOfFuelNotification();
                }
            }
            else
            {
                this.helperInputListener?.Stop();
                this.helperInputListener = null;

                // deactivate the item automatically on the client side
                GetPublicState(item).IsActive = false;
                Logger.Info($"Item light deselected, light turns off: {item}");
                // no need to sync with the server - it will automatically notice that the selected hotbar item has been changed
                //this.ClientTrySetActiveState(data.Item, setIsActive: false);
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

            if (ItemFuelRefillSystem.Instance.SharedGetCurrentActionState(Client.Characters.CurrentPlayerCharacter)
                != null)
            {
                // no need for notification - after refilling it will turn on automatically
                return;
            }

            // toggle state
            var setIsActive = !data.PublicState.IsActive;
            this.ClientTrySetActiveState(item, setIsActive);
        }

        protected virtual void ClientSetupSkeletonAnimation(
            bool isActive,
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            if (isActive)
            {
                skeletonRenderer.RemoveAnimationTrack(trackIndex: AnimationTrackIndexes.Extra);
                skeletonRenderer.SetAnimation(
                    trackIndex: AnimationTrackIndexes.Extra,
                    animationName: this.ActiveLightCharacterAnimationName,
                    isLooped: true);
            }
            else if (skeletonRenderer.GetCurrentAnimationName(AnimationTrackIndexes.Extra) != null)
            {
                // TODO: this is a hack - when an empty animation is added latest animation looping is breaking
                //skeletonRenderer.SetAnimationLoopMode(AnimationTrackIndexes.Extra, isLooped: false);
                skeletonRenderer.AddEmptyAnimation(AnimationTrackIndexes.Extra);
            }
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
            if (item.Container?.OwnerAsCharacter != ClientCurrentCharacterHelper.Character)
            {
                // item of another character
                return;
            }

            var publicState = data.PublicState;
            if (!publicState.IsActive)
            {
                return;
            }

            // check if item is in a hotbar selected slot, if not - make it not active
            var itemOwnerCharacter = item.Container?.OwnerAsCharacter;
            if (itemOwnerCharacter == null
                || item != itemOwnerCharacter.SharedGetPlayerSelectedHotbarItem())
            {
                Logger.Info(item + " is not in the hotbar selected slot - make it inactive");
                publicState.IsActive = false;
                return;
            }

            this.ItemFuelConfig.SharedTryConsumeFuel(item,
                                                     data.PrivateState,
                                                     data.DeltaTime,
                                                     out _);
        }

        protected virtual TextureResource GetCharacterTextureResource(IItem item, ICharacter character)
        {
            return GetPublicState(item).IsActive
                       ? this.CharacterTextureResourceActive
                       : this.CharacterTextureResourceInactive;
        }

        protected sealed override void PrepareProtoItem()
        {
            var lightConfig = new ItemLightConfig();
            var fuelConfig = new ItemFuelConfig();
            this.PrepareProtoItemLight(lightConfig, fuelConfig);
            this.ItemLightConfig = lightConfig.ToReadOnly();
            this.ItemFuelConfig = fuelConfig.ToReadOnly();
        }

        protected abstract void PrepareProtoItemLight(ItemLightConfig lightConfig, ItemFuelConfig fuelConfig);

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            this.ItemFuelConfig.ServerInitialize(data.PrivateState, data.IsFirstTimeInit);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var publicState = data.PublicState;
            if (!publicState.IsActive)
            {
                return;
            }

            var item = data.GameObject;
            ItemDurabilitySystem.ServerModifyDurability(item,
                                                        -this.DurabilityDecreasePerSecond * data.DeltaTime,
                                                        roundUp: true);

            // check if item is in a hotbar selected slot, if not - make it not active
            var itemOwnerCharacter = item.Container?.OwnerAsCharacter;
            if (itemOwnerCharacter == null
                || !itemOwnerCharacter.ServerIsOnline
                || item != itemOwnerCharacter.SharedGetPlayerSelectedHotbarItem())
            {
                Logger.Info(item + " is not in the hotbar selected slot or player is offline - make it inactive");
                publicState.IsActive = false;
                return;
            }

            ServerItemUseObserver.NotifyItemUsed(itemOwnerCharacter, item);

            this.ItemFuelConfig.SharedTryConsumeFuel(item, data.PrivateState, data.DeltaTime, out var isFuelRanOut);
            if (isFuelRanOut)
            {
                publicState.IsActive = false;
            }
        }

        [RemoteCallSettings(DeliveryMode.Default)]
        private void ServerRemote_SetMode(IItem item, bool setIsActive)
        {
            this.VerifyGameObject(item);

            var character = ServerRemoteContext.Character;
            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            var characterPublicState = PlayerCharacter.GetPublicState(character);

            var publicState = GetPublicState(item);
            if (publicState.IsActive == setIsActive)
            {
                Logger.Warning($"The same state is already set: isActive={setIsActive} for {item}", character);
                return;
            }

            var selectedHotbarItem = characterPublicState.SelectedItem;
            if (selectedHotbarItem != item)
            {
                if (item.Container == characterPrivateState.ContainerHotbar)
                {
                    // different hotbar item selected - select the required hotbar item
                    PlayerCharacter.SharedSelectHotbarSlotId(character, item.ContainerSlotId, isByPlayer: true);
                    selectedHotbarItem = characterPublicState.SelectedItem;
                }

                if (selectedHotbarItem != item)
                {
                    throw new Exception(
                        $"Only current light item active state could be switched: {item}, but current active item is {selectedHotbarItem}");
                }
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
    /// Base tool item prototype for light items.
    /// </summary>
    public abstract class ProtoItemToolLight
        : ProtoItemToolLight
            <ItemWithFuelPrivateState,
                ItemWithActiveFlagPublicState,
                EmptyClientState>
    {
    }
}