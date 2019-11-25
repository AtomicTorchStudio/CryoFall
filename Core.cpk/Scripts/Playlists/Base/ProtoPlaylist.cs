namespace AtomicTorch.CBND.CoreMod.Playlists
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// This is a base class for a music playlist.
    /// </summary>
    public abstract class ProtoPlaylist : ProtoEntity
    {
        public virtual double FadeOutDurationOnPlaylistChange => 3;

        public virtual PlayListMode Mode => PlayListMode.Sequential;

        public virtual double PauseBetweenTracksSeconds => 0;

        public IReadOnlyList<MusicTrack> Tracks { get; private set; }

        public int FindTrackIndex(MusicTrack track)
        {
            for (var index = 0; index < this.Tracks.Count; index++)
            {
                var entry = this.Tracks[index];
                if (ReferenceEquals(entry, track))
                {
                    return index;
                }
            }

            return -1;
        }

        protected abstract void PreparePlaylist(MusicTracks tracks);

        protected sealed override void PrepareProto()
        {
            base.PrepareProto();
            var tracks = new MusicTracks();

            try
            {
                this.PreparePlaylist(tracks);

                if (tracks.Count == 0)
                {
                    throw new Exception("No music tracks added into the playlist");
                }

                foreach (var track in tracks)
                {
                    if (track == null)
                    {
                        throw new Exception("Null reference provided as track");
                    }

                    if (!Api.Shared.IsFileExists(track.MusicResource))
                    {
                        throw new Exception("Music track doesn't exist: " + track.MusicResource);
                    }
                }

                this.Tracks = tracks.AsReadOnly();
            }
            catch
            {
                this.Tracks = new List<MusicTrack>(0).AsReadOnly();
                throw;
            }
        }

        protected class MusicTracks : List<MusicTrack>
        {
            public new MusicTracks Add(MusicTrack track)
            {
                base.Add(track);
                return this;
            }
        }
    }
}