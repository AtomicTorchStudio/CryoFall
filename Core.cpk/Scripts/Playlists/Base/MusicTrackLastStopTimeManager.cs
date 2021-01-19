namespace AtomicTorch.CBND.CoreMod.Playlists
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class MusicTrackLastStopTimeManager
    {
        private static readonly Dictionary<MusicResource, TrackStopTime> LastStopTime
            = new();

        public static void RememberLastTrack(MusicResource musicTrack, double stopAtPosition)
        {
            LastStopTime[musicTrack] = new TrackStopTime(stopAtPosition, Api.Client.Core.ClientRealTime);
        }

        public static bool TryGetLastStopTime(MusicResource musicTrack, out double stopAtPosition)
        {
            if (!LastStopTime.TryGetValue(musicTrack, out var result))
            {
                stopAtPosition = 0;
                return false;
            }

            LastStopTime.Remove(musicTrack);

            if (Api.Client.Core.ClientRealTime - result.ClientTimeLastPlayback
                > MusicPlaylistLastStopTimeManager.MaxDurationToRememberTrackPosition)
            {
                // it stopped too long ago, forget about this
                stopAtPosition = 0;
                return false;
            }

            stopAtPosition = result.StopAtPosition;
            return true;
        }

        public readonly struct TrackStopTime
        {
            public readonly double ClientTimeLastPlayback;

            public readonly double StopAtPosition;

            public TrackStopTime(double stopAtPosition, double clientTimeLastPlayback)
            {
                this.StopAtPosition = stopAtPosition;
                this.ClientTimeLastPlayback = clientTimeLastPlayback;
            }
        }
    }
}