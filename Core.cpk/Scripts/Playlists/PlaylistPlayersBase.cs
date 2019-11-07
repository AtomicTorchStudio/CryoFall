namespace AtomicTorch.CBND.CoreMod.Playlists
{
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi;

    public class PlaylistPlayersBase : ProtoPlaylist
    {
        public override PlayListMode Mode => PlayListMode.Random;

        [NotLocalizable]
        public override string Name => "Player's base playlist";

        public override double PauseBetweenTracksSeconds => 4 * 60; // 4 minutes

        protected override void PreparePlaylist(MusicTracks tracks)
        {
            tracks.Add(
                new MusicTrack(
                    "PlayersBase1",
                    isLooped: false,
                    fadeInDuration: 3,
                    fadeOutDuration: 5,
                    volume: 0.4));
        }
    }
}