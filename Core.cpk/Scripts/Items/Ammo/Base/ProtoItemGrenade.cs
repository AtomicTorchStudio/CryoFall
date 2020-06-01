namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.ItemExplosive;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoItemGrenade : ProtoItemAmmo, IAmmoGrenade
    {
        private DamageDescription damageDescriptionCharacters;

        public DamageDescription DamageDescriptionCharacters
            => this.damageDescriptionCharacters;

        public abstract double DamageRadius { get; }

        public ExplosionPreset ExplosionPreset { get; private set; }

        public virtual float ExplosionVolume => 0.5f;

        public abstract double FireRangeMax { get; }

        public double StructureDamage { get; private set; }

        public double StructureDefensePenetrationCoef { get; private set; }

        public sealed override void ClientOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition)
        {
            this.ClientExplodeAt(weaponCache, endPosition);
        }

        public sealed override void ClientOnObjectHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            ref bool isDamageStop)
        {
            isDamageStop = true;

            var explosionWorldPosition = SharedGetExplosionWorldPosition(damagedObject, hitData);
            this.ClientExplodeAt(weaponCache, explosionWorldPosition);
        }

        public virtual double ServerCalculateTotalDamageByExplosive(
            ICharacter byCharacter,
            IStaticWorldObject targetStaticWorldObject)
        {
            var structureExplosiveDefenseCoef =
                targetStaticWorldObject.ProtoStaticWorldObject.StructureExplosiveDefenseCoef;
            structureExplosiveDefenseCoef = MathHelper.Clamp(structureExplosiveDefenseCoef, 0, 1);

            var explosiveDefensePenetrationCoef = this.StructureDefensePenetrationCoef;
            explosiveDefensePenetrationCoef = MathHelper.Clamp(explosiveDefensePenetrationCoef, 0, 1);

            if (!PveSystem.SharedIsAllowStructureDamage(byCharacter,
                                                        targetStaticWorldObject,
                                                        showClientNotification: false))
            {
                return 0;
            }

            var damage = this.StructureDamage
                         * (1 - structureExplosiveDefenseCoef * (1 - explosiveDefensePenetrationCoef));
            return damage;
        }

        /// <summary>
        /// Please override <see cref="ServerOnCharacterHitByExplosion" />.
        /// </summary>
        public sealed override void ServerOnCharacterHit(
            ICharacter damagedCharacter,
            double damage,
            ref bool isDamageStop)
        {
        }

        public sealed override void ServerOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition)
        {
            this.ServerExplodeAt(weaponCache, endPosition, isHit: false);
        }

        public sealed override void ServerOnObjectHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            ref bool isDamageStop)
        {
            isDamageStop = true;

            var explosionWorldPosition = SharedGetExplosionWorldPosition(damagedObject, hitData);
            this.ServerExplodeAt(weaponCache, explosionWorldPosition, isHit: true);
        }

        public void ServerOnObjectHitByExplosion(IWorldObject worldObject, double damage, WeaponFinalCache weaponCache)
        {
            if (worldObject is ICharacter character)
            {
                this.ServerOnCharacterHitByExplosion(character, damage, weaponCache);
            }
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            foreach (var textureResource in this.ExplosionPreset.SpriteAtlasResources)
            {
                Client.Rendering.PreloadTextureAsync(textureResource);
            }
        }

        protected sealed override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 0;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1;
            rangeMax = this.FireRangeMax;

            this.PrepareExplosionPreset(out var explosionPreset);
            this.ExplosionPreset = explosionPreset
                                   ?? throw new Exception("No explosion preset provided");

            // prepare damage description for characters
            {
                var charDamageDistribution = new DamageDistribution();
                this.PrepareDamageDescriptionCharacters(
                    out var charDamageValue,
                    out var charArmorPiercingCoef,
                    out var charFinalDamageMultiplier,
                    charDamageDistribution);

                this.damageDescriptionCharacters = new DamageDescription(
                    damageValue: this.SharedPrepareStatDamageToCharacters(charDamageValue),
                    charArmorPiercingCoef,
                    charFinalDamageMultiplier,
                    rangeMax: this.DamageRadius,
                    charDamageDistribution);
            }

            // prepare damage properties for structures
            {
                this.PrepareDamageDescriptionStructures(
                    out var structureDamageValue,
                    out var structureDefencePenetrationCoef);

                this.StructureDamage = this.SharedPrepareStatDamageToStructures(structureDamageValue);
                this.StructureDefensePenetrationCoef = MathHelper.Clamp(structureDefencePenetrationCoef, 0, 1);
            }
        }

        protected abstract void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution);

        protected abstract void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef);

        protected abstract void PrepareExplosionPreset(out ExplosionPreset explosionPreset);

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
                collisionGroup: CollisionGroups.HitboxRanged);

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

        protected virtual void ServerOnCharacterHitByExplosion(
            ICharacter damagedCharacter,
            double damage,
            WeaponFinalCache weaponCache)
        {
        }

        protected virtual double SharedPrepareStatDamageToCharacters(double damageValue)
        {
            return damageValue * WeaponConstants.DamageExplosivesToCharactersMultiplier;
        }

        protected virtual double SharedPrepareStatDamageToStructures(double damageValue)
        {
            return damageValue * WeaponConstants.DamageExplosivesToStructuresMultiplier;
        }

        private static Vector2D SharedGetExplosionWorldPosition(IWorldObject damagedObject, WeaponHitData hitData)
        {
            var explosionWorldPosition = damagedObject switch
            {
                null                                   => hitData.FallbackTilePosition.ToVector2D(), // tile hit
                IDynamicWorldObject dynamicWorldObject => dynamicWorldObject.Position,
                _                                      => damagedObject.TilePosition.ToVector2D(),
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

        private void ClientExplodeAt(
            IProtoItemWeapon protoWeapon,
            Vector2D shotSourcePosition,
            Vector2D explosionWorldPosition)
        {
            var timeToHit = WeaponSystemClientDisplay.SharedCalculateTimeToHit(protoWeapon.FireTracePreset
                                                                               ?? this.FireTracePreset,
                                                                               shotSourcePosition,
                                                                               explosionWorldPosition);

            ClientTimersSystem.AddAction(timeToHit,
                                         () => ExplosionHelper.ClientExplode(explosionWorldPosition,
                                                                             this.ExplosionPreset,
                                                                             this.ExplosionVolume));
        }

        private void ClientRemote_OnExplosion(
            IProtoItemWeaponRanged protoWeapon,
            Vector2D shotSourcePosition,
            Vector2D explosionWorldPosition)
        {
            this.ClientExplodeAt(protoWeapon, shotSourcePosition, explosionWorldPosition);
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
                    shotSourcePosition,
                    character.Tile.Height,
                    endPosition,
                    character.PhysicsBody.PhysicsSpace,
                    anyCliffIsAnObstacle:
                    anyCliffIsAnObstacle))
            {
                ServerTimersSystem.AddAction(
                    timeToHit,
                    () =>
                    {
                        ExplosionHelper.ServerExplode(
                            character: character,
                            protoExplosive: this,
                            protoWeapon: protoWeapon,
                            explosionPreset: this.ExplosionPreset,
                            epicenterPosition: endPosition,
                            damageDescriptionCharacters: this.damageDescriptionCharacters,
                            physicsSpace: Server.World.GetPhysicsSpace(),
                            executeExplosionCallback: this.ServerExecuteExplosion);
                    });
            }

            // notify other characters about the explosion
            using var charactersObserving = Api.Shared.GetTempList<ICharacter>();
            var explosionEventRadius = Math.Max(protoWeapon.SoundPresetWeaponDistance.max,
                                                this.FireRangeMax)
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