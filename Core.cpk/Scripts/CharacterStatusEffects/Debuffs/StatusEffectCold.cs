namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class StatusEffectCold : ProtoStatusEffect
    {
        private static readonly List<IWorldObject> TempResult
            = new(capacity: 512);

        private HashSet<byte> serverColdProtoTileSessionIndices;

        private StatusEffectFrostbite serverStatusEffectFrostbite;

        public override string Description => "You are in a cold climate. Wearing warm clothes is a good idea.";

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Cold";

        public override double ServerUpdateIntervalSeconds => 0.1;

        /// <summary>
        /// Time to remove full effect intensity back to zero in case the environmental intensity is 0.
        /// </summary>
        protected double TimeToCoolDownToZeroSeconds => 10;

        /// <summary>
        /// Time to reach the full intensity in case the environmental intensity is 1.
        /// </summary>
        protected double TimeToReachFullIntensitySeconds => 30;

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectColdManager.TargetIntensity = 0;
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectColdManager.TargetIntensity = data.Intensity;
        }

        protected override void PrepareEffects(Effects effects)
        {
            // food consumption +25%
            effects.AddPercent(this, StatName.HungerRate, +25);

            // energy regeneration -25%
            effects.AddPercent(this, StatName.StaminaRegenerationPerSecond, -25);

            // movement speed -10%
            effects.AddPercent(this, StatName.MoveSpeed, -10);
        }

        protected override void PrepareProtoStatusEffect()
        {
            base.PrepareProtoStatusEffect();

            if (IsServer)
            {
                this.serverColdProtoTileSessionIndices = new HashSet<byte>(
                    FindProtoEntities<IProtoTileCold>().Select(p => p.SessionIndex));
                this.serverStatusEffectFrostbite = GetProtoEntity<StatusEffectFrostbite>();

                // setup timer (tick every frame)
                TriggerEveryFrame.ServerRegister(
                    callback: this.ServerGlobalUpdate,
                    name: "System." + this.ShortId);
            }
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            var defense = data.Character.SharedGetFinalStatValue(StatName.DefenseCold);

            defense *= 2.0; // defense over 0.5 (50%) is enough to completely insulate from cold effect accumulation
            defense = MathHelper.Clamp(defense, 0, 1);
            intensityToAdd *= 1 - defense;

            base.ServerAddIntensity(data, intensityToAdd);
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            this.ServerUpdateStatusEffectIntensity(data.Character, data.DeltaTime);

            if (data.Intensity >= 1.0)
            {
                // Cold status effect has reached 100% intensity, start adding the Frostbite status effect
                data.Character.ServerAddStatusEffect(this.serverStatusEffectFrostbite,
                                                     intensity: data.DeltaTime
                                                                * (1.0
                                                                   / StatusEffectFrostbite.EffectAccumulationDuration));
            }
            else
            {
                // Please note: the Frostbite status effect will remove itself automatically after cooldown
            }
        }

        /// <summary>
        /// Any active manufacturer and generator structure is assumed as a heat source preventing cold status effect.
        /// </summary>
        private static double ServerCalculateObjectHeatCoefficient(
            ICharacter character,
            IWorldObject worldObject)
        {
            if (worldObject.ProtoWorldObject
                is not IProtoObjectManufacturer
                and not IProtoObjectElectricityProducer)
            {
                return 0;
            }

            if (!worldObject.GetPublicState<ObjectManufacturerPublicState>().IsActive)
            {
                return 0;
            }

            var position = worldObject switch
            {
                IStaticWorldObject staticWorldObject => staticWorldObject.TilePosition.ToVector2D()
                                                        + staticWorldObject.ProtoStaticWorldObject.Layout.Center,
                IDynamicWorldObject dynamicWorldObject => dynamicWorldObject.Position,
                _                                      => throw new InvalidOperationException()
            };

            var distance = position.DistanceTo(character.Position);

            var maxDistance = 6;
            var minDistance = 3;
            var distanceCoef = (distance - minDistance) / (maxDistance - minDistance);
            var intensity = 1 - MathHelper.Clamp(distanceCoef, 0, 1);
            return intensity;
        }

        private double ServerCalculateEnvironmentalIntensity(ICharacter character)
        {
            if (!character.ServerIsOnline
                || character.IsNpc)
            {
                return 0;
            }

            if (!this.serverColdProtoTileSessionIndices.Contains(character.Tile.ProtoTileSessionIndex))
            {
                // not in a cold biome
                return 0;
            }

            // in a cold biome, but can be near an active heat source such as a campfire
            Server.World.GetWorldObjectsInView(character, TempResult, sortByDistance: false);

            var objectsHeat = 0.0;
            foreach (var worldObject in TempResult)
            {
                var objectHeatIntensity = ServerCalculateObjectHeatCoefficient(character,
                                                                               worldObject);
                objectsHeat += objectHeatIntensity;
            }

            return MathHelper.Clamp(1 - objectsHeat,
                                    min: 0,
                                    max: 1);
        }

        private void ServerGlobalUpdate()
        {
            // perform update once per second per player
            const double spreadDeltaTime = 1;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            PlayerCharacter.Instance
                           .EnumerateGameObjectsWithSpread(tempListPlayers.AsList(),
                                                           spreadDeltaTime: spreadDeltaTime,
                                                           Server.Game.FrameNumber,
                                                           Server.Game.FrameRate);

            foreach (var character in tempListPlayers.AsList())
            {
                if (!character.ServerIsOnline
                    || PlayerCharacter.GetPrivateState(character).IsDespawned)
                {
                    continue;
                }

                var statusEffects = character.GetPrivateState<BaseCharacterPrivateState>()
                                             .StatusEffects;

                var isEffectAlreadyAdded = false;
                foreach (var statusEffect in statusEffects)
                {
                    if (!ReferenceEquals(statusEffect.ProtoLogicObject, this))
                    {
                        continue;
                    }

                    // status effect is already added so it will be automatically updated
                    isEffectAlreadyAdded = true;
                    break;
                }

                if (isEffectAlreadyAdded)
                {
                    continue;
                }

                this.ServerUpdateStatusEffectIntensity(character, spreadDeltaTime);
            }
        }

        private void ServerUpdateStatusEffectIntensity(ICharacter character, double deltaTime)
        {
            var defense = character.SharedGetFinalStatValue(StatName.DefenseCold);
            defense *= 2.0; // defense over x2 is enough to completely insulate from cold effect accumulation
            defense = MathHelper.Clamp(defense, 0, 1);

            double environmentalColdIntensity;
            if (defense >= 1)
            {
                environmentalColdIntensity = 0;
            }
            else
            {
                environmentalColdIntensity = this.ServerCalculateEnvironmentalIntensity(character);
                if (environmentalColdIntensity > 0)
                {
                    environmentalColdIntensity *= 1 - defense;
                }
            }

            var currentIntensity = character.SharedGetStatusEffectIntensity(this);

            double delta;
            if (environmentalColdIntensity > 0)
            {
                // need to add the intensity
                var speed = environmentalColdIntensity / this.TimeToReachFullIntensitySeconds;
                delta = speed * deltaTime;
            }
            else
            {
                // need to reduce the intensity
                if (currentIntensity <= 0)
                {
                    return;
                }

                var speed = 1.0 / this.TimeToCoolDownToZeroSeconds;
                delta = -speed * deltaTime;
            }

            var newIntensity = currentIntensity + delta;
            character.ServerSetStatusEffectIntensity(this, intensity: newIntensity);
        }
    }
}