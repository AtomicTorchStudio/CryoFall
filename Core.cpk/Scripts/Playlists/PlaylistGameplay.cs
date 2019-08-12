// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.Playlists
{
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi;

    public class PlaylistGameplay : ProtoPlaylist
    {
        public override PlayListMode Mode => PlayListMode.Random;

        [NotLocalizable]
        public override string Name => "Gameplay playlist";

        public override double PauseBetweenTracksSeconds { get; } = 4 * 60; // 4 minutes

        protected override void PreparePlaylist(MusicTracks tracks)
        {
            const double fadeInDuration = 5,
                         fadeOutDuration = 10,
                         volume = 0.4;

            tracks.Add(
                      new MusicTrack(
                          "Gameplay1",
                          isLooped: false,
                          fadeInDuration: fadeInDuration,
                          fadeOutDuration: fadeOutDuration,
                          volume: volume))
                  .Add(
                      new MusicTrack(
                          "Gameplay2",
                          isLooped: false,
                          fadeInDuration: fadeInDuration,
                          fadeOutDuration: fadeOutDuration,
                          volume: volume))
                  .Add(
                      new MusicTrack(
                          "Gameplay3",
                          isLooped: false,
                          fadeInDuration: fadeInDuration,
                          fadeOutDuration: fadeOutDuration,
                          volume: volume));
        }
    }
}