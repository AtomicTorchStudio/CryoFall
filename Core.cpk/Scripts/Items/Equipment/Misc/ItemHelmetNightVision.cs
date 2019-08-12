namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.NightVision;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Lights;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ItemHelmetNightVision : ProtoItemEquipmentHeadWithLight
    {
        public override string Description =>
            "Lightweight tactical helmet with special night-vision goggles to enable near perfect vision at night. Requires disposable batteries.";

        public override uint DurabilityMax => 1200;

        public override bool IsHairVisible => false;

        public override string Name => "Night vision";

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            base.ClientSetupSkeleton(item, character, skeletonRenderer, skeletonComponents);

            if (character == null
                || !character.IsCurrentClientCharacter
                || !GetPublicState(item).IsActive)
            {
                return;
            }

            // setup night vision effect for current character
            var sceneObject = Client.Scene.GetSceneObject(character);
            var componentNightVisionEffect = sceneObject.AddComponent<ClientComponentNightVisionEffect>();
            skeletonComponents.Add(componentNightVisionEffect);

            // we need this to disable the light added by the PlayerCharacter class for the current character
            var componentLightInSkeleton = sceneObject.AddComponent<ClientComponentLightInSkeleton>();
            componentLightInSkeleton.IsEnabled = false;
            skeletonComponents.Add(componentLightInSkeleton);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.35,
                kinetic: 0.35,
                heat: 0.15,
                cold: 0.15,
                chemical: 0.15,
                electrical: 0.10,
                radiation: 0.10,
                psi: 0);

            // normal value override, we don't want it to be affected by armor multiplier later
            defense.Psi = 0.20 / defense.Multiplier;
        }

        protected override void PrepareProtoItemEquipmentHeadWithLight(
            ItemLightConfig lightConfig,
            ItemFuelConfig fuelConfig)
        {
            lightConfig.IsLightEnabled = false;

            fuelConfig.FuelCapacity = 1000;
            fuelConfig.FuelAmountInitial = 0;
            fuelConfig.FuelUsePerSecond = 1;
            fuelConfig.FuelProtoItemsList.AddAll<IProtoItemFuelElectricity>();
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric.Clone()
                                    .Replace(ItemSound.Use, "Items/Equipment/UseNightVision");
        }
    }
}