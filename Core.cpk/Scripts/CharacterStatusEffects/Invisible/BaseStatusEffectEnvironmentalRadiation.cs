namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Invisible
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Invisible.Client;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// This status effect express value of the environmental radiation.
    /// According to it the client could display the special visual/audio effect of radiation exposure.
    /// This is an invisible status effect - not displayed in the status effects list.
    /// </summary>
    public abstract class BaseStatusEffectEnvironmentalRadiation : ProtoStatusEffect
    {
        // Lookup area is a circle of the specified diameter.
        private const int EnvironmentalRadiationLookupAreaDiameter = 9;

        private Vector2Int[] serverTileOffsetsCircle;

        private IProtoZone serverZone;

        public override string Description => string.Empty;

        public override ITextureResource Icon => TextureResource.NoTexture;

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => this.ShortId;

        public override double VisibilityIntensityThreshold => double.MaxValue;

        protected abstract IProtoZone ServerZoneProto { get; }

        /// <summary>
        /// This method is called only once per second per each character in the radiation zone.
        /// RadiationIntensity is current environmental radiation (0-1).
        /// Please return value you would like to add to the radiation poisoning status effect.
        /// Please note this value will be decreased later at
        /// <see cref="StatusEffectRadiationPoisoning.ServerAddIntensity" />.
        /// </summary>
        protected abstract double CalculatePoisoningIntensityToAdd(
            double environmentalRadiationIntensity,
            double currentEffectIntensity);

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectEnvironmentalRadiationManager.TargetIntensity = 0;
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectEnvironmentalRadiationManager.TargetIntensity = data.Intensity;
        }

        protected override void PrepareProtoStatusEffect()
        {
            base.PrepareProtoStatusEffect();

            if (Api.IsClient)
            {
                return;
            }

            // cache the server zone prototype
            this.serverZone = this.ServerZoneProto;

            // cache the tile offsets
            // but select only every third tile (will help to reduce the load without damaging accuracy too much)
            this.serverTileOffsetsCircle = ShapeTileOffsetsHelper
                                           .GenerateOffsetsCircle(EnvironmentalRadiationLookupAreaDiameter)
                                           .ToArray();

            this.serverTileOffsetsCircle = ShapeTileOffsetsHelper.SelectOffsetsWithRate(
                this.serverTileOffsetsCircle,
                rate: 3);

            // setup timer (tick every frame)
            TriggerEveryFrame.ServerRegister(
                callback: this.ServerGlobalUpdate,
                name: "System." + this.ShortId);
        }

        private void ServerGlobalUpdate()
        {
            var zone = this.serverZone.ServerZoneInstance;
            var totalCells = this.serverTileOffsetsCircle.Length;

            // perform update once per second per player
            const double spreadDeltaTime = 1;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            PlayerCharacter.Instance
                           .EnumerateGameObjectsWithSpread(tempListPlayers.AsList(),
                                                           spreadDeltaTime: spreadDeltaTime,
                                                           Server.Game.FrameNumber,
                                                           Server.Game.FrameRate);

            foreach (var character in tempListPlayers)
            {
                if (!character.ServerIsOnline
                    || character.GetPublicState<ICharacterPublicState>()
                                .IsDead)
                {
                    continue;
                }

                var characterTilePosition = character.TilePosition;
                var cellsInZone = 0;

                foreach (var tileOffset in this.serverTileOffsetsCircle)
                {
                    if (zone.IsContainsPosition(characterTilePosition.AddAndClamp(tileOffset)))
                    {
                        cellsInZone++;
                    }
                }

                var radiationIntensity = cellsInZone / (double)totalCells;
                character.ServerSetStatusEffectIntensity(this, radiationIntensity);

                if (radiationIntensity <= 0)
                {
                    continue;
                }

                var currentEffectIntensity = character.SharedGetStatusEffectIntensity<StatusEffectRadiationPoisoning>();

                // add poisoning
                var poisoningIntensityToAdd =
                    this.CalculatePoisoningIntensityToAdd(radiationIntensity, currentEffectIntensity)
                    * spreadDeltaTime;

                character.ServerAddStatusEffect<StatusEffectRadiationPoisoning>(
                    intensity: poisoningIntensityToAdd);
            }
        }
    }
}