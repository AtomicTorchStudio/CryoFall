namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Vehicle
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemVehicleKeiniteArtillery : ProtoItemVehicleWeaponGrenadeLauncher, IProtoExplosive
    {
        public override ushort AmmoCapacity => 100;

        public override ushort AmmoConsumptionPerShot => 10;

        public override double AmmoReloadDuration => 4;

        public override string CharacterAnimationAimingName => "WeaponAiming1Hand";

        public override double CharacterAnimationAimingRecoilDuration => 0.45;

        public override string CharacterAnimationAimingRecoilName => "WeaponShooting1Hand";

        public override double CharacterAnimationAimingRecoilPower => 0.5;

        public override double CharacterAnimationAimingRecoilPowerAddCoef
            => 1 / 2.5; // full recoil power will be gained on third shot

        public double DamageRadius => 1.575;

        public override string Description =>
            "Launches large exploding capsules containing a cocktail of acids and toxins that will deal damage not only to living things but even structures.";

        public override uint DurabilityMax => 500;

        public ExplosionPreset ExplosionPreset { get; private set; }

        public override double FireInterval => 1 / 2.0; // 2.0 per second

        public override string Name => "Keinite cannon";

        public override double SpecialEffectProbability => 1.0;

        public double StructureDamage { get; private set; }

        public double StructureDefensePenetrationCoef { get; private set; }

        public float VolumeExplosion => 0.5f;

        public override string WeaponAttachmentName => "TurretLeft";

        public override VehicleWeaponHardpoint WeaponHardpoint => VehicleWeaponHardpoint.Exotic;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillVehicles>();

        public virtual double ServerCalculateTotalDamageByExplosive(
            ICharacter byCharacter,
            IStaticWorldObject targetStaticWorldObject)
        {
            var structureExplosiveDefenseCoef =
                targetStaticWorldObject.ProtoStaticWorldObject.StructureExplosiveDefenseCoef;
            structureExplosiveDefenseCoef = MathHelper.Clamp(structureExplosiveDefenseCoef, 0, 1);

            var explosiveDefensePenetrationCoef = this.StructureDefensePenetrationCoef;
            explosiveDefensePenetrationCoef = MathHelper.Clamp(explosiveDefensePenetrationCoef, 0, 1);

            if (!PveSystem.SharedIsAllowStaticObjectDamage(byCharacter,
                                                           targetStaticWorldObject,
                                                           showClientNotification: false))
            {
                return 0;
            }

            var damage = this.StructureDamage
                         * (1 - structureExplosiveDefenseCoef * (1 - explosiveDefensePenetrationCoef));
            return damage;
        }

        public void ServerOnObjectHitByExplosion(IWorldObject worldObject, double damage, WeaponFinalCache weaponCache)
        {
            if (worldObject is ICharacter character)
            {
                this.ServerOnCharacterHitByExplosion(character, damage, weaponCache);
            }
        }

        public override void SharedOnHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            out bool isDamageStop)
        {
            isDamageStop = true;

            if (IsServer)
            {
                var explosionWorldPosition = SharedGetExplosionWorldPosition(damagedObject, hitData);
                this.ServerExplodeAt(weaponCache, explosionWorldPosition, isHit: true);
            }
            else
            {
                var explosionWorldPosition = SharedGetExplosionWorldPosition(damagedObject, hitData);
                this.ClientExplodeAt(weaponCache, explosionWorldPosition);
            }
        }

        public override void SharedOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition)
        {
            if (IsServer)
            {
                this.ServerExplodeAt(weaponCache, endPosition, isHit: false);
            }
            else
            {
                this.ClientExplodeAt(weaponCache, endPosition);
            }
        }

        protected virtual void ClientExplodeAt(
            IProtoItemWeapon protoWeapon,
            Vector2D shotSourcePosition,
            Vector2D explosionWorldPosition)
        {
            var timeToHit = WeaponSystemClientDisplay.SharedCalculateTimeToHit(protoWeapon.FireTracePreset
                                                                               ?? this.FireTracePreset,
                                                                               shotSourcePosition,
                                                                               explosionWorldPosition);

            ClientTimersSystem.AddAction(timeToHit,
                                         () => SharedExplosionHelper.ClientExplode(explosionWorldPosition,
                                             this.ExplosionPreset,
                                             this.VolumeExplosion));
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            foreach (var textureResource in this.ExplosionPreset.SpriteAtlasResources)
            {
                Client.Rendering.PreloadTextureAsync(textureResource);
            }
        }

        protected override string GenerateIconPath()
        {
            return "Items/Weapons/Vehicle/" + this.GetType().Name;
        }

        protected void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            damageValue = 100;
            defencePenetrationCoef = 0;
        }

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new(
                initialSequence: new[] { 0.0, 1.2, 2.0 },
                cycledSequence: new[] { 2.5 });
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.ExoticWeaponPoison;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.None)
                       .Set(textureScreenOffset: (100, -89));
        }

        protected override void PrepareProtoGrenadeLauncher(out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos)
        {
            compatibleAmmoProtos = SharedGetCompatibleAmmoProto();
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = SharedGetCompatibleAmmoProto();

            this.PrepareExplosionPreset(out var explosionPreset);
            this.ExplosionPreset = explosionPreset;

            this.ExplosionPreset = explosionPreset
                                   ?? throw new Exception("No explosion preset provided");

            overrideDamageDescription = new DamageDescription(
                damageValue: 48,
                armorPiercingCoef: 0.4,
                finalDamageMultiplier: 1.2,
                rangeMax: 10,
                new DamageDistribution(DamageType.Chemical, 1));

            // prepare damage properties for structures
            {
                this.PrepareDamageDescriptionStructures(
                    out var structureDamageValue,
                    out var structureDefencePenetrationCoef);

                this.StructureDamage = this.SharedPrepareStatDamageToStructures(structureDamageValue);
                this.StructureDefensePenetrationCoef = MathHelper.Clamp(structureDefencePenetrationCoef, 0, 1);
            }
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedExotic;
        }

        protected virtual void ServerExecuteExplosion(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            WeaponFinalCache weaponFinalCache)
        {
            WeaponExplosionSystem.ServerProcessExplosionCircle(
                positionEpicenter: positionEpicenter,
                physicsSpace: physicsSpace,
                damageDistanceMax: this.DamageRadius,
                weaponFinalCache: weaponFinalCache,
                damageOnlyDynamicObjects: false,
                isDamageThroughObstacles: false,
                callbackCalculateDamageCoefByDistanceForStaticObjects: CalcDamageCoefByDistance,
                callbackCalculateDamageCoefByDistanceForDynamicObjects: CalcDamageCoefByDistance,
                collisionGroups: new[] { CollisionGroups.HitboxRanged, CollisionGroup.Default });

            double CalcDamageCoefByDistance(double distance)
            {
                var distanceThreshold = 0.5;
                if (distance <= distanceThreshold)
                {
                    return 1;
                }

                distance -= distanceThreshold;
                distance = Math.Max(0, distance);

                var maxDistance = this.DamageRadius;
                maxDistance -= distanceThreshold;
                maxDistance = Math.Max(0, maxDistance);

                return 1 - Math.Min(distance / maxDistance, 1);
            }
        }

        protected void ServerOnCharacterHitByExplosion(
            ICharacter damagedCharacter,
            double damage,
            WeaponFinalCache weaponCache)
        {
            if (damage < 1)
            {
                return;
            }

            // 100% chance to add bleeding and toxins
            damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.05);
            damagedCharacter.ServerAddStatusEffect<StatusEffectToxins>(intensity: 0.05);
        }

        protected virtual double SharedPrepareStatDamageToStructures(double damageValue)
        {
            return damageValue * WeaponConstants.DamageExplosivesToStructuresMultiplier;
        }

        private static IEnumerable<IAmmoKeinite> SharedGetCompatibleAmmoProto()
        {
            return GetAmmoOfType<IAmmoKeinite>();
        }

        private static Vector2D SharedGetExplosionWorldPosition(IWorldObject damagedObject, WeaponHitData hitData)
        {
            var explosionWorldPosition = damagedObject switch
            {
                null                                   => hitData.FallbackTilePosition.ToVector2D(), // tile hit
                IDynamicWorldObject dynamicWorldObject => dynamicWorldObject.Position,
                _                                      => damagedObject.TilePosition.ToVector2D()
            };

            return explosionWorldPosition + hitData.HitPoint;
        }

        private void ClientExplodeAt(WeaponFinalCache weaponCache, Vector2D explosionWorldPosition)
        {
            var protoWeapon = weaponCache.ProtoWeapon;
            var shotSourcePosition = WeaponSystemClientDisplay.SharedCalculateWeaponShotWorldPositon(
                weaponCache.Character,
                protoWeapon,
                weaponCache.Character.ProtoCharacter,
                weaponCache.Character.Position,
                hasTrace: true);

            this.ClientExplodeAt(protoWeapon, shotSourcePosition, explosionWorldPosition);
        }

        private void ClientRemote_OnExplosion(
            IProtoItemWeaponRanged protoWeapon,
            Vector2D shotSourcePosition,
            Vector2D explosionWorldPosition)
        {
            this.ClientExplodeAt(protoWeapon, shotSourcePosition, explosionWorldPosition);
        }

        private void PrepareExplosionPreset(out ExplosionPreset explosionPreset)
        {
            explosionPreset = ExplosionPresets.KeiniteSmall;
        }

        private void ServerExplodeAt(WeaponFinalCache weaponCache, Vector2D endPosition, bool isHit)
        {
            var character = weaponCache.Character;
            var protoWeapon = (IProtoItemWeaponRanged)weaponCache.ProtoWeapon;

            var shotSourcePosition = WeaponSystemClientDisplay.SharedCalculateWeaponShotWorldPositon(
                character,
                protoWeapon,
                character.ProtoCharacter,
                character.Position,
                hasTrace: true);

            if (isHit)
            {
                // offset end position a bit towards the source position
                // this way the explosion will definitely happen outside the hit object
                endPosition -= 0.1 * (endPosition - shotSourcePosition).Normalized;
            }

            var timeToHit = WeaponSystemClientDisplay.SharedCalculateTimeToHit(
                protoWeapon.FireTracePreset
                ?? this.FireTracePreset,
                shotSourcePosition,
                endPosition);

            // We're using a check here similar to the one in WeaponSystem.
            // Ensure that player can hit objects only on the same height level
            // and can fire through over the pits (the cliffs of the lower heights).
            var anyCliffIsAnObstacle = character.Tile.Height
                                       != Server.World.GetTile(endPosition.ToVector2Ushort()).Height;

            if (!WeaponSystem.SharedHasTileObstacle(
                    character.Position,
                    character.Tile.Height,
                    endPosition,
                    character.PhysicsBody.PhysicsSpace,
                    anyCliffIsAnObstacle: anyCliffIsAnObstacle))
            {
                ServerTimersSystem.AddAction(
                    timeToHit,
                    () =>
                    {
                        SharedExplosionHelper.ServerExplode(
                            character: character,
                            protoExplosive: this,
                            protoWeapon: protoWeapon,
                            explosionPreset: this.ExplosionPreset,
                            epicenterPosition: endPosition,
                            damageDescriptionCharacters: this.OverrideDamageDescription,
                            physicsSpace: Server.World.GetPhysicsSpace(),
                            executeExplosionCallback: this.ServerExecuteExplosion);
                    });
            }

            // notify other characters about the explosion
            using var charactersObserving = Api.Shared.GetTempList<ICharacter>();
            var explosionEventRadius = Math.Max(protoWeapon.SoundPresetWeaponDistance.max,
                                                this.OverrideDamageDescription.RangeMax)
                                       + this.DamageRadius;

            Server.World.GetCharactersInRadius(endPosition.ToVector2Ushort(),
                                               charactersObserving,
                                               radius: (byte)Math.Min(byte.MaxValue, explosionEventRadius),
                                               onlyPlayers: true);

            charactersObserving.Remove(character);

            this.CallClient(charactersObserving.AsList(),
                            _ => _.ClientRemote_OnExplosion(protoWeapon,
                                                            shotSourcePosition,
                                                            endPosition));
        }
    }
}