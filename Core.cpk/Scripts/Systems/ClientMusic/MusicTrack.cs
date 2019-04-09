namespace AtomicTorch.CBND.CoreMod.Systems.ClientMusic
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;

    public class MusicTrack
    {
        public readonly double FadeInDuration;

        public readonly double FadeOutDuration;

        public readonly bool IsLooped;

        public readonly MusicResource MusicResource;

        public readonly double Volume;

        /// <param name="musicResource">Music resource.</param>
        /// <param name="isLooped">Is looped.</param>
        /// <param name="fadeInDuration">Fade in duration (in seconds).</param>
        /// <param name="fadeOutDuration">Fade out duration (in seconds).</param>
        public MusicTrack(
            MusicResource musicResource,
            bool isLooped,
            double fadeInDuration,
            double fadeOutDuration,
            double volume = 1)
        {
            this.MusicResource = musicResource ?? throw new ArgumentNullException(nameof(musicResource));
            this.IsLooped = isLooped;
            this.FadeInDuration = fadeInDuration;
            this.FadeOutDuration = fadeOutDuration;
            if (volume < 0
                || volume > 1)
            {
                throw new ArgumentException("Volume must be in [0;1] range", nameof(volume));
            }

            this.Volume = volume;
        }

        /// <param name="musicResource">Music local file path (in Content/Music folder).</param>
        /// <param name="isLooped">Is looped.</param>
        /// <param name="fadeInDuration">Fade in duration (in seconds).</param>
        /// <param name="fadeOutDuration">Fade out duration (in seconds).</param>
        public MusicTrack(
            string musicLocalFilePath,
            bool isLooped,
            double fadeInDuration,
            double fadeOutDuration,
            double volume = 1)
            : this(
                new MusicResource(musicLocalFilePath),
                isLooped,
                fadeInDuration,
                fadeOutDuration,
                volume)
        {
        }

        public bool IsSame(MusicTrack track)
        {
            return this.MusicResource == track.MusicResource
                   && this.FadeInDuration == track.FadeInDuration
                   && this.FadeOutDuration == track.FadeOutDuration
                   && this.IsLooped == track.IsLooped;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}: {2}, {3}: {4}, {5}: {6}",
                                 this.MusicResource,
                                 nameof(this.IsLooped),
                                 this.IsLooped,
                                 nameof(this.FadeInDuration),
                                 this.FadeInDuration,
                                 nameof(this.FadeOutDuration),
                                 this.FadeOutDuration);
        }
    }
}