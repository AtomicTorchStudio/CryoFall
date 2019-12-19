namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class StatusEffectHeat : ProtoRadiantStatusEffect
    {
        public const double DamagePerSecondByIntensity = 10;

        private const int EnvironmentalTileHeatLookupAreaDiameter = 9;

        private static readonly IWorldServerService ServerWorld = IsServer ? Server.World : null;

        private ProtoTile serverProtoTileLava;

        private ProtoTile serverProtoTileVolcano;

        private Vector2Int[] serverTileOffsetsCircle;

        public override string Description =>
            "You are exposed to a high level of heat from a nearby heat source. Immediately leave the area to prevent further damage.";

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Heat";

        protected override StatName DefenseStatName => StatName.DefenseHeat;

        /// <summary>
        /// Time to remove full effect intensity back to zero in case the environmental intensity is 0.
        /// </summary>
        protected override double TimeToCoolDownToZeroSeconds => 3;

        /// <summary>
        /// Time to reach the full intensity in case the environmental intensity is 1.
        /// </summary>
        protected override double TimeToReachFullIntensitySeconds => 4;

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectHeatManager.TargetIntensity = 0;
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectHeatManager.TargetIntensity = data.Intensity;
        }

        protected override void PrepareEffects(Effects effects)
        {
            // add info to tooltip that this effect deals damage
            effects.AddValue(this, StatName.VanityContinuousDamage, 1);
        }

        protected override void PrepareProtoStatusEffect()
        {
            base.PrepareProtoStatusEffect();

            // cache the tile offsets
            // but select only every third tile (will help to reduce the load without damaging accuracy too much)
            this.serverTileOffsetsCircle = ShapeTileOffsetsHelper
                                           .GenerateOffsetsCircle(EnvironmentalTileHeatLookupAreaDiameter)
                                           .ToArray();

            this.serverTileOffsetsCircle = ShapeTileOffsetsHelper.SelectOffsetsWithRate(
                this.serverTileOffsetsCircle,
                rate: 3);

            this.serverProtoTileLava = Api.GetProtoEntity<TileLava>();
            this.serverProtoTileVolcano = Api.GetProtoEntity<TileVolcanic>();
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            // reduce directly applied status effect (e.g. from gunshots) based on armor (same as used for environmental effect)
            var defense = data.Character.SharedGetFinalStatValue(this.DefenseStatName);
            defense = MathHelper.Clamp(defense, 0, 1);
            intensityToAdd *= 1 - DefensePotentialMultiplier * defense;

            base.ServerAddIntensity(data, intensityToAdd);
        }

        protected override double ServerCalculateEnvironmentalIntensityAroundCharacter(ICharacter character)
        {
            var objectsEnviromentalIntensity = base.ServerCalculateEnvironmentalIntensityAroundCharacter(character);
            var tilesEnviromentalIntensity = this.ServerCalculateTileEnvironmentalIntensityAroundCharacter(character);
            return Math.Max(objectsEnviromentalIntensity, tilesEnviromentalIntensity);
        }

        protected override double ServerCalculateObjectEnvironmentalIntensity(
            ICharacter character,
            IWorldObject worldObject)
        {
            if (!(worldObject.ProtoWorldObject is IProtoObjectHeatSource protoHeatSource))
            {
                return 0;
            }

            Vector2D position;
            switch (worldObject)
            {
                case IStaticWorldObject staticWorldObject:
                    position = staticWorldObject.TilePosition.ToVector2D()
                               + staticWorldObject.ProtoStaticWorldObject.Layout.Center;
                    break;
                case IDynamicWorldObject dynamicWorldObject:
                    position = dynamicWorldObject.Position;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var distance = position.DistanceTo(character.Position);

            var maxDistance = protoHeatSource.HeatRadiusMax;
            var minDistance = protoHeatSource.HeatRadiusMin;
            var distanceCoef = (distance - minDistance) / (maxDistance - minDistance);
            var intensity = 1 - MathHelper.Clamp(distanceCoef, 0, 1);

            return intensity * MathHelper.Clamp(protoHeatSource.HeatIntensity, 0, 1);
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            base.ServerUpdate(data);

            var damage = DamagePerSecondByIntensity
                         * Math.Pow(data.Intensity, 1.5)
                         * data.DeltaTime;

            // modify damage based on effect multiplier
            damage *= data.Character.SharedGetFinalStatMultiplier(StatName.HeatEffectMultiplier);

            // modify damage based on armor
            // divided by 2 because otherwise many armor pieces would give practically complete immunity to heat
            // so 100% armor would give 50% reduction in damage
            var defenseHeat = data.Character.SharedGetFinalStatValue(StatName.DefenseHeat);
            damage *= Math.Max(0, 1 - defenseHeat / 2.0);

            data.CharacterCurrentStats.ServerReduceHealth(damage, data.StatusEffect);
        }

        // calculate closest lava tile position and heat intensity from it
        private double ServerCalculateTileEnvironmentalIntensityAroundCharacter(ICharacter character)
        {
            if (!character.ServerIsOnline)
            {
                return 0;
            }

            var tileVolcanoSessionIndex = this.serverProtoTileVolcano.SessionIndex;
            var tileLavaSessionIndex = this.serverProtoTileLava.SessionIndex;

            var characterCurrentTileSessionIndex = character.Tile.ProtoTileSessionIndex;

            if (characterCurrentTileSessionIndex != tileVolcanoSessionIndex
                && characterCurrentTileSessionIndex != tileLavaSessionIndex)
            {
                // process lava heat only for players in volcano biome
                return 0;
            }

            var characterTilePosition = character.TilePosition;
            var closestDistanceSqr = int.MaxValue;

            foreach (var tileOffset in this.serverTileOffsetsCircle)
            {
                var tilePosition = characterTilePosition.AddAndClamp(tileOffset);
                var tile = ServerWorld.GetTile(tilePosition, logOutOfBounds: false);
                if (!tile.IsValidTile
                    || tileLavaSessionIndex != tile.ProtoTileSessionIndex)
                {
                    continue;
                }

                var distanceSqr = tileOffset.X * tileOffset.X
                                  + tileOffset.Y * tileOffset.Y;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                }
            }

            if (closestDistanceSqr
                >= (EnvironmentalTileHeatLookupAreaDiameter
                    * EnvironmentalTileHeatLookupAreaDiameter
                    * 0.5
                    * 0.5))
            {
                // no heat - too far from any lava
                return 0;
            }

            var heatIntensity = (EnvironmentalTileHeatLookupAreaDiameter * 0.5 - Math.Sqrt(closestDistanceSqr))
                                / (EnvironmentalTileHeatLookupAreaDiameter * 0.5);

            heatIntensity *= 2;
            heatIntensity = Math.Min(heatIntensity, 1);

            return heatIntensity;
        }
    }
}