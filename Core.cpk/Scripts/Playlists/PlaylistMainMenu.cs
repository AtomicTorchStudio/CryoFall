// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.Playlists
{
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi;

    public class PlaylistMainMenu : ProtoPlaylist
    {
        public override double FadeOutDurationOnPlaylistChange => 2;

        [NotLocalizable]
        public override string Name => "Main menu playlist";

        protected override void PreparePlaylist(MusicTracks tracks)
        {
            tracks.Add(
                new MusicTrack(
                    "MainMenu1",
                    isLooped: true,
                    fadeInDuration: 1,
                    fadeOutDuration: 2,
                    volume: 1));
        }
    }
}