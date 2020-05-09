namespace AtomicTorch.CBND.CoreMod.Items.Equipment.SuperHeavyArmor
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.NightVision;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ItemSuperHeavyArmor
        : ProtoItemEquipmentFullBody
          <ItemWithDurabilityPrivateState,
              EmptyPublicState,
              ItemSuperHeavyArmor.ClientState>,
          IProtoItemEquipmentHeadWithLight
    {
        public override string Description =>
            "The ultimate defense. This battle armor will protect you against anything that comes at you, but at a cost of reduced mobility.";

        public override uint DurabilityMax => 1500;

        public override bool IsHairVisible => false;

        // TODO: there is an issue with long beard, add sprite mask clipping and restore this to true
        public override bool IsHeadVisible => false;

        public IReadOnlyItemFuelConfig ItemFuelConfig { get; }
            = new ItemFuelConfig();

        public IReadOnlyItemLightConfig ItemLightConfig { get; }
            = new ItemLightConfig() { IsLightEnabled = false };

        public override ObjectMaterial Material => ObjectMaterial.Metal;

        public override string Name => "Super heavy armor";

        public bool ClientCanStartRefill(IItem item)
        {
            return false;
        }

        public override void ClientOnItemContainerSlotChanged(IItem item)
        {
            ClientAutoLightToggle(item);
        }

        public void ClientOnRefilled(IItem item, bool isCurrentHotbarItem)
        {
        }

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents,
            bool isPreview)
        {
            base.ClientSetupSkeleton(item,
                                     character,
                                     skeletonRenderer,
                                     skeletonComponents,
                                     isPreview);

            if (!isPreview
                && item.IsInitialized
                && (character?.IsCurrentClientCharacter ?? false))
            {
                ClientRefreshNightVisionState(item);
            }
        }

        public void ClientToggleLight(IItem item)
        {
            var character = item.Container.OwnerAsCharacter;
            if (character is null
                || !character.IsCurrentClientCharacter)
            {
                return;
            }

            Api.GetProtoEntity<ItemHelmetNightVision>()
               .SoundPresetItem
               .PlaySound(ItemSound.Use);

            var clientState = GetClientState(item);
            clientState.IsNightVisionActive = !clientState.IsNightVisionActive;

            ClientRefreshNightVisionState(item);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            ClientAutoLightToggle(data.GameObject);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.90,
                kinetic: 0.90,
                heat: 0.90,
                cold: 0.90,
                chemical: 0.90,
                electrical: 0.90,
                radiation: 0.40,
                psi: 0.40);
        }

        protected override void PrepareEffects(Effects effects)
        {
            // cannot run
            effects.AddPercent(this, StatName.MoveSpeedRunMultiplier, -100);

            // movement speed
            effects.AddPercent(this, StatName.MoveSpeed, -10);
        }

        private static void ClientAutoLightToggle(IItem item)
        {
            if (!item.IsInitialized)
            {
                return;
            }

            var character = item.Container.OwnerAsCharacter;
            var clientState = GetClientState(item);

            if (character is null
                || !character.IsCurrentClientCharacter
                || item.Container != character.SharedGetPlayerContainerEquipment())
            {
                clientState.IsNightVisionActive = false;
                return;
            }

            // the item is put into the player equipment container
            // enable the light automatically during the night
            var isLightRequired = TimeOfDaySystem.IsNight;

            if (clientState.IsNightVisionActive == isLightRequired)
            {
                return;
            }

            clientState.IsNightVisionActive = isLightRequired;
            // active state changed - invalidate skeleton renderer (so it will be rebuilt)
            character.ClientInvalidateSkeletonRenderer();
        }

        private static void ClientRefreshNightVisionState(IItem item)
        {
            if (!item.IsInitialized)
            {
                return;
            }

            var character = item.Container.OwnerAsCharacter;
            if (character is null
                || !character.IsCurrentClientCharacter)
            {
                return;
            }

            var clientState = GetClientState(item);
            var skeletonComponents = character.GetClientState<PlayerCharacterClientState>().SkeletonComponents;
            var component = skeletonComponents.Find(t => t is ClientComponentNightVisionEffect);

            if (!clientState.IsNightVisionActive)
            {
                // turn effect off
                if (component != null)
                {
                    component.Destroy();
                    skeletonComponents.Remove(component);
                }

                return;
            }

            // turn effect on
            if (component != null)
            {
                return;
            }

            component = character.ClientSceneObject
                                 .AddComponent<ClientComponentNightVisionEffect>();
            skeletonComponents.Add(component);
        }

        public class ClientState : BaseClientState
        {
            public bool IsNightVisionActive { get; set; }
        }
    }
}