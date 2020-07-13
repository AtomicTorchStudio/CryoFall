namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.NightVision;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ItemHelmetNightVisionAdvanced : ProtoItemEquipmentHeadWithLight
    {
        public override string Description =>
            "Improved version of the night-vision enabled tactical helmet, which allows for perfect vision at night. Requires disposable batteries.";

        public override uint DurabilityMax => 1200;

        public override bool IsHairVisible => false;

        public override string Name => "Advanced night vision";

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents,
            bool isPreview)
        {
            base.ClientSetupSkeleton(item, character, skeletonRenderer, skeletonComponents, isPreview);

            if (isPreview
                || character == null
                || !character.IsCurrentClientCharacter
                || !GetPublicState(item).IsActive)
            {
                return;
            }

            // setup night vision effect for current character
            var sceneObject = character.ClientSceneObject;
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
                impact: 0.50,
                kinetic: 0.50,
                explosion: 0.50,
                heat: 0.25,
                cold: 0.20,
                chemical: 0.20,
                radiation: 0.20,
                psi: 0.0);

            // normal value override, we don't want it to be affected by armor multiplier later
            defense.Psi = 0.25 / defense.Multiplier;
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