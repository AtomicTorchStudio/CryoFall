namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TileAmbientSoundProvider
    {
        private readonly AmbientSoundPreset[] ambientSoundPresetsDay;

        private readonly AmbientSoundPreset[] ambientSoundPresetsNight;

        public TileAmbientSoundProvider(
            AmbientSoundPreset daySoundPreset,
            AmbientSoundPreset nightSoundPreset = null)
        {
            // prepare tile sounds presets
            var ambientSoundPresetDay = daySoundPreset
                                        ?? new AmbientSoundPreset(SoundResource.NoSound);

            var ambientSoundPresetNight = nightSoundPreset ?? ambientSoundPresetDay;
            this.ambientSoundPresetsDay = new[] { ambientSoundPresetDay };
            this.ambientSoundPresetsNight = new[] { ambientSoundPresetNight };
        }

        public virtual AmbientSoundPreset[] ClientGetAmbientSound(Tile tile, bool isDay)
        {
            return isDay
                       ? this.ambientSoundPresetsDay
                       : this.ambientSoundPresetsNight;
        }
    }
}