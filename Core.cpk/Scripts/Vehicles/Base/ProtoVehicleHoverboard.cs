namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Invisible;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoVehicleHoverboard
        <TVehiclePrivateState,
         TVehiclePublicState,
         TVehicleClientState>
        : ProtoVehicle
            <TVehiclePrivateState,
                TVehiclePublicState,
                TVehicleClientState>
        where TVehiclePrivateState : VehicleMechPrivateState, new()
        where TVehiclePublicState : VehiclePublicState, new()
        where TVehicleClientState : VehicleClientState, new()
    {
        public override byte CargoItemsSlotsCount => 0;

        public override bool IsAllowCreatureDamageWhenNoPilot => false;

        public override bool IsArmorBarDisplayedWhenPiloted => false;

        public override bool IsHeavyVehicle => false;

        public override bool IsPlayersHotbarAndEquipmentItemsAllowed => true;

        public abstract Color LightColor { get; }

        public abstract Size2F LightLogicalSize { get; }

        public abstract Vector2D LightPositionOffset { get; }

        public abstract Size2F LightSize { get; }

        public override ITextureResource MapIcon => new TextureResource("Icons/MapExtras/VehicleHoverboard");

        public override double MaxDistanceToInteract => 0.5;

        public override SoundResource SoundResourceVehicleDismount { get; }
            = new("Objects/Vehicles/Hoverboard/Dismount");

        public override SoundResource SoundResourceVehicleMount { get; }
            = new("Objects/Vehicles/Hoverboard/Mount");

        public override double StatMoveSpeedRunMultiplier => 1.0; // no run mode

        public abstract TextureResource TextureResourceHoverboard { get; }

        public abstract TextureResource TextureResourceHoverboardLight { get; }

        protected abstract SoundResource EngineSoundResource { get; }

        protected abstract double EngineSoundVolume { get; }

        public override void ServerOnPilotDamage(
            WeaponFinalCache weaponCache,
            IDynamicWorldObject vehicle,
            ICharacter pilotCharacter,
            double damageApplied)
        {
            if (damageApplied > 0)
            {
                // drop from hoverboard on any character damage
                VehicleSystem.ServerCharacterExitCurrentVehicle(pilotCharacter, force: true);
            }
        }

        // reduce hoverboard movement speed in radtown
        public override double SharedGetMoveSpeedMultiplier(IDynamicWorldObject vehicle, ICharacter characterPilot)
        {
            if (characterPilot is null
                || !characterPilot.IsInitialized
                || characterPilot.IsNpc
                || (IsClient
                    && !characterPilot.IsCurrentClientCharacter))
            {
                return 1;
            }

            var statusEffects = characterPilot.GetPrivateState<BaseCharacterPrivateState>()
                                              .StatusEffects;
            foreach (var statusEffect in statusEffects)
            {
                var protoStatusEffect = statusEffect.ProtoGameObject;
                if (!(protoStatusEffect is BaseStatusEffectEnvironmentalRadiation))
                {
                    continue;
                }

                var intensity = statusEffect.GetPublicState<StatusEffectPublicState>().Intensity;
                if (intensity > 0.15)
                {
                    return 0.5;
                }

                break;
            }

            return 1.0;
        }

        protected virtual BaseClientComponentLightSource ClientCreateActiveEngineLightSource(
            IDynamicWorldObject vehicle)
        {
            return ClientLighting.CreateLightSourceSpot(
                vehicle.ClientSceneObject,
                color: this.LightColor,
                size: this.LightSize,
                logicalSize: this.LightLogicalSize,
                spritePivotPoint: (0.5, 0.5),
                positionOffset: this.LightPositionOffset);
        }

        protected override void ClientSetupRendering(ClientInitializeData data)
        {
            var vehicle = data.GameObject;
            var publicState = data.PublicState;

            // setup light source for the active hoverboard engine
            var lightSourceActiveEngine = this.ClientCreateActiveEngineLightSource(vehicle);
            RefreshLightSource();

            data.PublicState.ClientSubscribe(_ => _.PilotCharacter,
                                             RefreshLightSource,
                                             data.ClientState);

            void RefreshLightSource()
            {
                lightSourceActiveEngine.IsEnabled = publicState.PilotCharacter is not null;
            }

            var componentHoverboardVisualManager = vehicle.ClientSceneObject
                                                          .AddComponent<ComponentHoverboardVisualManager>();
            componentHoverboardVisualManager
                .Setup(vehicle,
                       lightSourceActiveEngine,
                       textureResourceHoverboard: this.TextureResourceHoverboard,
                       textureResourceLight: this.TextureResourceHoverboardLight,
                       this.StatMoveSpeed);
            data.ClientState.SpriteRenderer = componentHoverboardVisualManager.SpriteRendererHoverboard;

            HoverboardEngineSoundEmittersManager.CreateSoundEmitter(vehicle,
                                                                    this.EngineSoundResource,
                                                                    this.EngineSoundVolume);
        }

        protected override void PrepareProtoVehicleDestroyedExplosionPreset(
            out double damageRadius,
            out ExplosionPreset explosionPreset,
            out DamageDescription damageDescriptionCharacters)
        {
            damageRadius = 2.1;
            explosionPreset = ExplosionPresets.Large;

            damageDescriptionCharacters = new DamageDescription(
                damageValue: 75,
                armorPiercingCoef: 0,
                finalDamageMultiplier: 1,
                rangeMax: damageRadius,
                damageDistribution: new DamageDistribution(DamageType.Kinetic, 1));
        }

        protected override void ServerOnDynamicObjectDamageApplied(
            WeaponFinalCache weaponCache,
            IDynamicWorldObject targetObject,
            float previousStructurePoints,
            float currentStructurePoints)
        {
            base.ServerOnDynamicObjectDamageApplied(weaponCache,
                                                    targetObject,
                                                    previousStructurePoints,
                                                    currentStructurePoints);

            // drop from hoverboard on any vehicle damage
            var pilotCharacter = GetPublicState(targetObject).PilotCharacter;
            if (pilotCharacter is not null)
            {
                VehicleSystem.ServerCharacterExitCurrentVehicle(pilotCharacter, force: true);
            }
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IDynamicWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            var result = base.SharedCalculateDamageByWeapon(weaponCache,
                                                            damagePreMultiplier,
                                                            targetObject,
                                                            out obstacleBlockDamageCoef);
            if (result > 0)
            {
                return result;
            }

            obstacleBlockDamageCoef = 0;
            return 0;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var physicsBody = data.PhysicsBody;
            if (data.PublicState.PilotCharacter is null)
            {
                // no pilot
                physicsBody.AddShapeRectangle(size: (0.9, 0.6),
                                              offset: (-0.45, -0.4),
                                              group: CollisionGroups.ClickArea)
                           .AddShapeRectangle(size: (0.9, 0.6),
                                              offset: (-0.45, -0.4),
                                              group: CollisionGroups.HitboxMelee);
            }
            else
            {
                // like human legs collider but much larger
                var radius = 0.35;
                var colliderY = -0.115;

                // Please note: the top edge of the collider should match the top edge of standing player character,
                // otherwise player character's hitbox may go a bit outside the wall above when the hoverboard
                // is too close to it.
                AddShapes(offsetY: 0.04);
                AddShapes(offsetY: -0.1);

                void AddShapes(double offsetY)
                {
                    physicsBody.AddShapeCircle(
                        radius / 2,
                        center: (-radius / 2, offsetY + colliderY),
                        CollisionGroups.CharacterOrVehicle);

                    physicsBody.AddShapeCircle(
                        radius / 2,
                        center: (radius / 2, offsetY + colliderY),
                        CollisionGroups.CharacterOrVehicle);

                    physicsBody.AddShapeRectangle(
                        size: (radius, radius),
                        offset: (-radius / 2, offsetY + colliderY - radius / 2),
                        CollisionGroups.CharacterOrVehicle);
                }
            }
        }

        protected override void SharedGetSkeletonProto(
            IDynamicWorldObject gameObject,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale)
        {
            // no skeleton, only a hoverboard sprite underneath the player character
            protoSkeleton = null;
        }
    }

    public abstract class ProtoVehicleHoverboard
        : ProtoVehicleHoverboard
            <VehicleMechPrivateState,
                VehiclePublicState,
                VehicleClientState>
    {
        public override float ObjectSoundRadius => 1;
    }
}