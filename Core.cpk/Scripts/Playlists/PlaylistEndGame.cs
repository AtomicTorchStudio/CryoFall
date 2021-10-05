namespace AtomicTorch.CBND.CoreMod.Playlists
{
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi;

    public class PlaylistEndGame : ProtoPlaylist
    {
        public override double FadeOutDurationOnPlaylistChange => 2;

        [NotLocalizable]
        public override string Name => "End game menu playlist";

        protected override void PreparePlaylist(MusicTracks tracks)
        {
            tracks.Add(
                new MusicTrack(
                    "MainMenu1", // re-using the main menu track
                    isLooped: true,
                    fadeInDuration: 2,
                    fadeOutDuration: 2,
                    volume: 1));
        }
    }
}