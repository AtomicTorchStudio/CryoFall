namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TileForestAmbientSoundProvider : TileAmbientSoundProvider
    {
        public TileForestAmbientSoundProvider(
            AmbientSoundPreset daySoundPresetPlains,
            AmbientSoundPreset daySoundPresetForest,
            AmbientSoundPreset nightSoundPresetPlains,
            AmbientSoundPreset nightSoundPresetForest)
            : base(daySoundPresetPlains, nightSoundPresetPlains)
        {
            Api.Assert(daySoundPresetPlains is not null,   $"{nameof(daySoundPresetPlains)} cannot be null");
            Api.Assert(daySoundPresetForest is not null,   $"{nameof(daySoundPresetForest)} cannot be null");
            Api.Assert(nightSoundPresetPlains is not null, $"{nameof(nightSoundPresetPlains)} cannot be null");
            Api.Assert(nightSoundPresetForest is not null, $"{nameof(nightSoundPresetForest)} cannot be null");

            // combine sound presets for case when there are trees or bushes nearby
            this.AmbientSoundPresetsDayWithTrees = new[] { daySoundPresetPlains, daySoundPresetForest };
            this.AmbientSoundPresetsNightWithTrees = new[] { nightSoundPresetPlains, nightSoundPresetForest };
        }

        protected AmbientSoundPreset[] AmbientSoundPresetsDayWithTrees { get; }

        protected AmbientSoundPreset[] AmbientSoundPresetsNightWithTrees { get; }

        public override AmbientSoundPreset[] ClientGetAmbientSound(Tile tile, bool isDay)
        {
            var hasTreesNearby = HasTreesNearby(tile);

            if (hasTreesNearby)
            {
                return isDay
                           ? this.AmbientSoundPresetsDayWithTrees
                           : this.AmbientSoundPresetsNightWithTrees;
            }

            return base.ClientGetAmbientSound(tile, isDay);
        }

        private static bool HasTreesNearby(Tile tile)
        {
            foreach (var staticWorldObject in tile.StaticObjects)
            {
                var protoStaticWorldObject = staticWorldObject.ProtoStaticWorldObject;
                if (protoStaticWorldObject is IProtoObjectTree
                    || protoStaticWorldObject is IProtoObjectBush)
                {
                    return true;
                }
            }

            return false;
        }
    }
}