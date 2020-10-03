namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TileAmbientSoundProvider
    {
        public TileAmbientSoundProvider(
            AmbientSoundPreset daySoundPreset,
            AmbientSoundPreset nightSoundPreset = null)
        {
            // prepare tile sounds presets
            var ambientSoundPresetDay = daySoundPreset;
            if (ambientSoundPresetDay is null)
            {
                ambientSoundPresetDay = new AmbientSoundPreset(SoundResource.NoSound);
            }

            var ambientSoundPresetNight = nightSoundPreset ?? ambientSoundPresetDay;
            this.AmbientSoundPresetsDay = new[] { ambientSoundPresetDay };
            this.AmbientSoundPresetsNight = new[] { ambientSoundPresetNight };
        }

        protected AmbientSoundPreset[] AmbientSoundPresetsDay { get; }

        protected AmbientSoundPreset[] AmbientSoundPresetsNight { get; }

        public virtual AmbientSoundPreset[] ClientGetAmbientSound(Tile tile, bool isDay)
        {
            return isDay
                       ? this.AmbientSoundPresetsDay
                       : this.AmbientSoundPresetsNight;
        }
    }
}