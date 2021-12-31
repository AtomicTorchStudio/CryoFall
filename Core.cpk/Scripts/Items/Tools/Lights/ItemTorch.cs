namespace AtomicTorch.CBND.CoreMod.Items.Tools.Lights
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ItemTorch : ProtoItemToolLight
    {
        public const string NotificationBurnedOut = "The torch has burned out.";

        private static readonly TextureResource CharacterTextureResourceBurned =
            new("Characters/Tools/ItemTorchBurned");

        private static readonly TextureAtlasResource TextureAtlasFire
            = new("Characters/Tools/ItemTorchFire",
                  columns: 4,
                  rows: 2,
                  isTransparent: true);

        public override string Description =>
            "This will keep you sane at night. Make sure you have a spare just in case.";

        public override uint DurabilityMax =>
            0; // Important! No durability limit here, the item will be destroyed when the fuel amount is zero

        public override string Name => "Torch";

        public override double ServerUpdateIntervalSeconds => 1;

        protected override string ActiveLightCharacterAnimationName => "Torch";

        public override void ServerOnCharacterDeath(IItem item, bool isEquipped, out bool shouldDrop)
        {
            if (PveSystem.ServerIsPvE)
            {
                shouldDrop = true; // drop (if the PvE mechanic supports it)
                return;
            }

            // destroy torches in PvP to prevent "noob loot" (when a loot pile contains only a torch)
            Server.Items.DestroyItem(item);
            shouldDrop = false;
        }

        protected override BaseClientComponentLightSource ClientCreateLightSource(
            IItem item,
            ICharacter character)
        {
            var lightSource = base.ClientCreateLightSource(item, character);

            // add light flickering
            character.ClientSceneObject
                     .AddComponent<ClientComponentLightSourceEffectFlickering>()
                     .Setup(lightSource,
                            flickeringPercents: 10,
                            flickeringChangePercentsPerSecond: 70);

            return lightSource;
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected override void ClientSetupSkeletonAnimation(
            bool isActive,
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            base.ClientSetupSkeletonAnimation(isActive, item, character, skeletonRenderer, skeletonComponents);

            if (!isActive)
            {
                return;
            }

            // create fire sprite renderer
            var sceneObject = skeletonRenderer.SceneObject;
            var fireRenderer = Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                TextureResource.NoTexture,
                // draw origin is calculated so that the center
                // of the flame sprite will always stay on the end of the torch item
                spritePivotPoint: (0.5, 0.277),
                // please note: the torch X axis ("Weapon" slot) is oriented up because torch is held this way!
                positionOffset: (0.35, 0),
                scale: 0.9);

            var fireAnimator = sceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
            fireAnimator.Setup(
                fireRenderer,
                ClientComponentSpriteSheetAnimator.CreateAnimationFrames(TextureAtlasFire),
                isLooped: true,
                frameDurationSeconds: 1 / 15.0,
                randomizeInitialFrame: true);

            var slotNameClone = "Weapon_Torch";
            skeletonRenderer.CloneSlot("Weapon", slotNameClone);
            skeletonRenderer.SetAttachmentRenderer(slotNameClone,
                                                   "WeaponMelee",
                                                   fireRenderer,
                                                   applyBoneRotation: false);

            skeletonRenderer.SetAttachment(slotNameClone, "WeaponMelee");

            skeletonComponents.Add(fireRenderer);
            skeletonComponents.Add(fireAnimator);
        }

        protected override void ClientShowOutOfFuelNotification()
        {
            // do nothing, we have a special notification for that event (see ClientRemote_TorchBurned)
        }

        protected override TextureResource GetCharacterTextureResource(IItem item, ICharacter character)
        {
            return CharacterTextureResourceBurned;
        }

        protected override void PrepareProtoItemLight(ItemLightConfig lightConfig, ItemFuelConfig fuelConfig)
        {
            lightConfig.Color = LightColors.Torch;
            lightConfig.ScreenOffset = (17, 147);
            lightConfig.Size = 16;

            fuelConfig.FuelCustomIcon = new TextureResource("Icons/IconFire");
            fuelConfig.FuelCapacity = 6.5 * 60;                     // 6.5 minutes
            fuelConfig.FuelAmountInitial = fuelConfig.FuelCapacity; // torch is spawned with the full fuel amount
            fuelConfig.FuelUsePerSecond = 1;
            // fuel list is not populated - the item is spawned fully charged and destroyed when charge is 0
            //config.FuelList = ...
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            if (data.PrivateState.FuelAmount > 0)
            {
                base.ServerUpdate(data);
                return;
            }

            // the torch has burned out - destroy the item
            var item = data.GameObject;
            var owner = item.Container.OwnerAsCharacter;

            Server.Items.DestroyItem(item);

            if (owner is not null)
            {
                // notify owner
                this.CallClient(owner, _ => _.ClientRemote_TorchBurned());
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_TorchBurned()
        {
            NotificationSystem.ClientShowNotification(
                NotificationBurnedOut,
                color: NotificationColor.Neutral,
                icon: this.Icon,
                playSound: false);

            SoundPresetsHelper.SharedGetItemSoundPreset(this)
                              .PlaySound(ItemSound.Unequip);
        }
    }
}