namespace AtomicTorch.CBND.CoreMod.Playlists
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class MusicPlaylistLastStopTimeManager
    {
        // Last play position will be remembered by this manager for not longer than this time duration.
        public const double MaxDurationToRememberTrackPosition = 1 * 60; // 1 minute

        private static readonly Dictionary<ProtoPlaylist, PlaylistStopTime> LastStopTime
            = new Dictionary<ProtoPlaylist, PlaylistStopTime>();

        public static void RememberLastTrack(ProtoPlaylist playlist, MusicTrack lastMusicTrack)
        {
            LastStopTime[playlist] = new PlaylistStopTime(lastMusicTrack, Api.Client.Core.ClientRealTime);
        }

        public static bool TryGetLastPlayedMusicTrack(ProtoPlaylist playlist, out MusicTrack lastMusicTrack)
        {
            if (!LastStopTime.TryGetValue(playlist, out var result))
            {
                lastMusicTrack = null;
                return false;
            }

            LastStopTime.Remove(playlist);

            if (Api.Client.Core.ClientRealTime - result.ClientTimeLastPlayback
                > MaxDurationToRememberTrackPosition)
            {
                // it stopped too long ago, forget about this
                lastMusicTrack = null;
                return false;
            }

            lastMusicTrack = result.LastMusicTrack;
            return true;
        }

        public readonly struct PlaylistStopTime
        {
            public readonly double ClientTimeLastPlayback;

            public readonly MusicTrack LastMusicTrack;

            public PlaylistStopTime(MusicTrack lastMusicTrack, double clientTimeLastPlayback)
            {
                this.LastMusicTrack = lastMusicTrack;
                this.ClientTimeLastPlayback = clientTimeLastPlayback;
            }
        }
    }
}