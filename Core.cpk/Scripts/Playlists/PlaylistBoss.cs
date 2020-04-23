namespace AtomicTorch.CBND.CoreMod.Playlists
{
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi;

    public class PlaylistBoss : ProtoPlaylist
    {
        public override double FadeOutDurationOnPlaylistChange => 3;

        public override PlayListMode Mode => PlayListMode.Random;

        [NotLocalizable]
        public override string Name => "Boss playlist";

        protected override void PreparePlaylist(MusicTracks tracks)
        {
            tracks.Add(
                new MusicTrack(
                    // reuse the base raid music
                    "BaseRaid1",
                    isLooped: true,
                    fadeInDuration: 1.5,
                    fadeOutDuration: 5,
                    volume: 0.35));
        }
    }
}