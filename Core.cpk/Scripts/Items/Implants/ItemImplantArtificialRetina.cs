namespace AtomicTorch.CBND.CoreMod.Items.Implants
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ItemImplantArtificialRetina : ProtoItemEquipmentImplant
    {
        public override string Description =>
            "Special highly photosensitive retina layer with HDR support, which grants almost perfect vision at night.";

        public override string Name => "Artificial retina implant";

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            base.ClientSetupSkeleton(item, character, skeletonRenderer, skeletonComponents);

            if (character == null
                || !character.IsCurrentClientCharacter)
            {
                return;
            }

            // setup artificial retina effect for current character
            var sceneObject = Client.Scene.GetSceneObject(character);
            var componentNightVisionEffect = sceneObject.AddComponent<ClientComponentArtificialRetinaEffect>();
            skeletonComponents.Add(componentNightVisionEffect);
        }

        /// <summary>
        /// It increases ambient light for the lighting system during the night.
        /// </summary>
        private class ClientComponentArtificialRetinaEffect : ClientComponent
        {
            private const double AdditionalAmbientLight = 0.7;

            private const double AdditionalAmbientLightAdditiveFraction = 0.35;

            private double lastValuePrimary;

            private double lastValueSecondary;

            public ClientComponentArtificialRetinaEffect()
                : base(isLateUpdateEnabled: true)
            {
            }

            public override void LateUpdate(double deltaTime)
            {
                this.Refresh();
            }

            protected override void OnDisable()
            {
                this.Refresh();
            }

            protected override void OnEnable()
            {
                this.lastValuePrimary = this.lastValueSecondary = 0;
                this.Refresh();
            }

            private void Refresh()
            {
                ClientComponentLightingRenderer.AdditionalAmbientLight -= this.lastValuePrimary;
                ClientComponentLightingRenderer.AdditionalAmbightLightAdditiveFraction -= this.lastValueSecondary;

                if (!this.IsEnabled)
                {
                    this.lastValuePrimary = this.lastValueSecondary = 0;
                    return;
                }

                var multiplier = TimeOfDaySystem.NightFraction * 2.2
                                 + Math.Max(TimeOfDaySystem.DuskFraction, TimeOfDaySystem.DawnFraction) / 2;

                multiplier = Math.Min(1, multiplier);

                this.lastValuePrimary = multiplier * AdditionalAmbientLight;
                this.lastValueSecondary = multiplier * AdditionalAmbientLightAdditiveFraction;
                ClientComponentLightingRenderer.AdditionalAmbientLight += this.lastValuePrimary;
                ClientComponentLightingRenderer.AdditionalAmbightLightAdditiveFraction += this.lastValueSecondary;
            }
        }
    }
}