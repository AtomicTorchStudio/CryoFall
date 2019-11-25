namespace AtomicTorch.CBND.CoreMod.Playlists
{
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi;

    public class PlaylistBaseRaid : ProtoPlaylist
    {
        public override PlayListMode Mode => PlayListMode.Random;

        [NotLocalizable]
        public override string Name => "Base raid playlist";

        protected override void PreparePlaylist(MusicTracks tracks)
        {
            tracks.Add(
                new MusicTrack(
                    "BaseRaid1",
                    isLooped: true,
                    fadeInDuration: 1.5,
                    fadeOutDuration: 3,
                    volume: 0.35));
        }
    }
}