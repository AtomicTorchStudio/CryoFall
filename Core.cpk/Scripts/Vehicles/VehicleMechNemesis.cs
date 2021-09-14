namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.NightVision;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class VehicleMechNemesis : ProtoVehicleMech
    {
        /// <summary>
        /// Determines how many of the mech HP regenerated per second.
        /// </summary>
        public const double StructurePointsRegenerationPerSecond = 2.0;

        /// <summary>
        /// Determines how often the mech HP are regenerated.
        /// It's best to keep a reasonable rate (within 1-10 seconds range).
        /// </summary>
        private const double StructurePointsRegenerationInterval = 3.0; // every 3 seconds

        public override byte CargoItemsSlotsCount => 24; // same as T3 Skipper mech

        public override string Description =>
            "Experimental design for mechanized battle armor. Features automatic regeneration of structural damage and powerful exotic weapons.";

        public override ushort EnergyUsePerSecondIdle => 70;

        public override ushort EnergyUsePerSecondMoving => 300;

        public override BaseItemsContainerMechEquipment EquipmentItemsContainerType
            => Api.GetProtoEntity<ContainerMechEquipmentNemesis>();

        public override bool HasVehicleLights => true;

        public override string Name => "Nemesis";

        public override double PhysicsBodyAccelerationCoef => 4;

        public override double PhysicsBodyFriction => 10;

        public override double StatMoveSpeed => 2.0;

        public override float StructurePointsMax => 500;

        public override double VehicleWorldHeight => 2.0;

        public override void ClientSetupSkeleton(
            IDynamicWorldObject vehicle,
            IProtoCharacterSkeleton protoSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            var publicState = GetPublicState(vehicle);

            if (!vehicle.IsInitialized
                || publicState.PilotCharacter is null
                || !publicState.PilotCharacter.IsCurrentClientCharacter)
            {
                return;
            }

            var componentNightVision = vehicle.ClientSceneObject
                                              .AddComponent<ClientComponentNightVisionEffectMechNemesis>();

            publicState.ClientSubscribe(
                _ => _.IsLightsEnabled,
                _ => RefreshNightVision(),
                subscriptionOwner: GetClientState(vehicle));

            RefreshNightVision();

            void RefreshNightVision()
            {
                componentNightVision.IsEnabled = publicState.IsLightsEnabled;
            }
        }

        protected override BaseClientComponentLightSource ClientCreateLightSource(IDynamicWorldObject vehicle)
        {
            throw new InvalidOperationException();
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.50,
                kinetic: 0.50,
                explosion: 0.50,
                heat: 0.40,
                cold: 0.40,
                chemical: 0.50,
                radiation: 0.0,
                psi: 0.0);
        }

        protected override void PrepareDismountPoints(List<Vector2D> dismountPoints)
        {
            dismountPoints.Add((0, -0.36));     // down
            dismountPoints.Add((-0.45, -0.36)); // down-left
            dismountPoints.Add((0.45, -0.36));  // down-right
            dismountPoints.Add((0, 0.36));      // up
            dismountPoints.Add((-0.7, 0));      // left
            dismountPoints.Add((0.7, 0));       // right
            dismountPoints.Add((-0.45, 0.36));  // up-left
            dismountPoints.Add((0.45, 0.36));   // up-right
        }

        protected override void PrepareProtoVehicle(
            InputItems buildRequiredItems,
            InputItems repairStageRequiredItems,
            out int repairStagesCount)
        {
            buildRequiredItems
                .Add<ItemKeinite>(100)
                .Add<ItemPragmiumHeart>(3)
                .Add<ItemTyrantHeart>(3)
                .Add<ItemVialBiomaterial>(50);

            repairStagesCount = 0; // this mech has health regeneration and cannot be repaired in any other way

            if (IsServer)
            {
                TriggerEveryFrame.ServerRegister(
                    callback: this.ServerUpdateStructurePointsRegeneration,
                    name: "System." + this.ShortId + ".ArmorRegeneration");
            }
        }

        protected override void PrepareProtoVehicleDestroyedExplosionPreset(
            out double damageRadius,
            out ExplosionPreset explosionPreset,
            out DamageDescription damageDescriptionCharacters)
        {
            damageRadius = 2.1;
            explosionPreset = ExplosionPresets.VeryLarge;

            damageDescriptionCharacters = new DamageDescription(
                damageValue: 25,
                armorPiercingCoef: 0.25,
                finalDamageMultiplier: 1,
                rangeMax: damageRadius,
                damageDistribution: new DamageDistribution(DamageType.Explosion, 1));
        }

        protected override void PrepareProtoVehicleLightConfig(ItemLightConfig lightConfig)
        {
            lightConfig.IsLightEnabled = false;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            base.SharedCreatePhysics(data);

            var physicsBody = data.PhysicsBody;
            if (data.PublicState.PilotCharacter is null)
            {
                // no pilot
                physicsBody.AddShapeRectangle(size: (0.9, 1.5),
                                              offset: (-0.45, -0.4),
                                              group: CollisionGroups.ClickArea);
            }
        }

        protected override void SharedGetSkeletonProto(
            IDynamicWorldObject gameObject,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale)
        {
            protoSkeleton = Api.GetProtoEntity<SkeletonMechNemesis>();
        }

        private void ServerUpdateStructurePointsRegeneration()
        {
            using var tempListVehicles = Api.Shared.GetTempList<IDynamicWorldObject>();
            this.EnumerateGameObjectsWithSpread(tempListVehicles.AsList(),
                                                spreadDeltaTime: StructurePointsRegenerationInterval,
                                                Server.Game.FrameNumber,
                                                Server.Game.FrameRate);
            foreach (var vehicle in tempListVehicles.AsList())
            {
                var publicState = GetPublicState(vehicle);
                var newStructurePoints = publicState.StructurePointsCurrent
                                         + (float)(StructurePointsRegenerationPerSecond
                                                   * StructurePointsRegenerationInterval);
                publicState.StructurePointsCurrent = Math.Min(newStructurePoints, this.StructurePointsMax);
            }
        }

        public class ClientComponentNightVisionEffectMechNemesis : ClientComponentNightVisionEffect
        {
            protected override EffectResource EffectResource
                => new("PostEffects/NightVisionMechNemesis");
        }
    }
}