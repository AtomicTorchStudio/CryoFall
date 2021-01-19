namespace AtomicTorch.CBND.CoreMod.Systems.ClientMusic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientOptions.Audio;
    using AtomicTorch.CBND.CoreMod.Playlists;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class ClientMusicSystem
    {
        private static readonly IAudioClientService Audio = Api.Client.Audio;

        private static readonly IComponentMusicSource ComponentMusicSource;

        // we don't need any logging as the system works fine, but it's easy to restore logging
        private static readonly ILogger Logger = Api.NullLogger; // Api.Logger

        private static readonly List<MusicTrack> Queue = new();

        private static ProtoPlaylist currentPlaylist;

        private static int currentPlaylistTrackIndex = -1;

        // ReSharper disable once InconsistentNaming
        private static CurrentMusicTrack CurrentTrack;

        private static double delayToNextTrackRemains;

        static ClientMusicSystem()
        {
            ComponentMusicSource = Audio.CreateMusicSource();
            Api.Client.Scene.CreateSceneObject(nameof(ComponentMusicSource))
               .AddComponent<ClientComponentMusicSystemUpdater>();
        }

        public static ProtoPlaylist CurrentPlaylist
        {
            get => currentPlaylist;
            set
            {
                if (value is not null
                    && value.Tracks.Count == 0)
                {
                    // no tracks in this playlist
                    // (should be impossible as PrepareProto check will throw an exception in ProtoPlaylist)
                    value = null;
                }

                if (currentPlaylist == value)
                {
                    return;
                }

                currentPlaylist = value;
                Logger.Important("Current music playlist changed to " + (value?.ToString() ?? "<no playlist>"));

                ClearQueue();

                if (currentPlaylist is null)
                {
                    return;
                }

                currentPlaylistTrackIndex = -1;
                SelectNextPlaylistTrackIndex();
                PlayNow(currentPlaylist.Tracks[currentPlaylistTrackIndex]);
            }
        }

        public static void EnsureMusicPlaying()
        {
            // TODO: ensure the music is playing so player can adjust the volume and hear the difference
            delayToNextTrackRemains = 0;
        }

        private static void ClearQueue()
        {
            delayToNextTrackRemains = 0;
            if (Queue.Count == 0)
            {
                return;
            }

            Queue.Clear();
            Logger.Important("Clearing music queue");
        }

        private static void PlayNext(MusicTrack track)
        {
            Queue.Add(track);
            Logger.Important("Music queue: enqueued: " + track);
        }

        private static void PlayNow(MusicTrack track)
        {
            if (Queue.Count == 1
                && Queue[0].IsSame(track))
            {
                Logger.Important("Music queue: not changed - already playing: " + track);
                return;
            }

            ClearQueue();
            Queue.Add(track);
            Logger.Important("Music queue: scheduled to play now: " + track);
        }

        private static void SelectNextPlaylistTrackIndex()
        {
            if (MusicPlaylistLastStopTimeManager.TryGetLastPlayedMusicTrack(currentPlaylist, out var lastMusicTrack))
            {
                Logger.Important("Playlist " + currentPlaylist.ShortId + " will resume track " + lastMusicTrack);
                currentPlaylistTrackIndex = currentPlaylist.FindTrackIndex(lastMusicTrack);
                return;
            }

            switch (currentPlaylist.Mode)
            {
                case PlayListMode.Sequential:
                    // select next track
                    currentPlaylistTrackIndex++;
                    if (currentPlaylistTrackIndex >= currentPlaylist.Tracks.Count)
                    {
                        // restart playing playlist from the beginning
                        currentPlaylistTrackIndex = 0;
                    }

                    return;

                case PlayListMode.Random:
                    // select random track (but do not repeat the already played track)
                    int nextPlaylistTrackIndex;

                    do
                    {
                        nextPlaylistTrackIndex = RandomHelper.Next(0, currentPlaylist.Tracks.Count);
                    }
                    while (currentPlaylistTrackIndex == nextPlaylistTrackIndex
                           && currentPlaylist.Tracks.Count > 1);

                    currentPlaylistTrackIndex = nextPlaylistTrackIndex;
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void Tick(double deltaTime)
        {
            if (AudioOptionVolumeAmbient.CurrentVolumeForAmbientSounds < 0.3)
            {
                // ambient sounds are too quiet - ensure there are no delays between the music tracks
                delayToNextTrackRemains = 0;
            }
            else if (delayToNextTrackRemains > 0)
            {
                delayToNextTrackRemains -= deltaTime;
                if (delayToNextTrackRemains <= 0)
                {
                    Logger.Important("Music pause between tracks completed");
                }
            }

            if (Audio.VolumeMusic <= 0
                || Audio.VolumeMaster <= 0)
            {
                // no music should be playing
                if (CurrentTrack is not null)
                {
                    CurrentTrack.RequestStop();
                    CurrentTrack = null;
                }

                if (ComponentMusicSource.MusicResource is not null)
                {
                    ComponentMusicSource.Stop();
                    ComponentMusicSource.MusicResource = null;
                }

                return;
            }

            // music should be playing
            if (CurrentTrack is not null)
            {
                var isFirstTrackOfQueue = CurrentTrack.MusicTrack == Queue.FirstOrDefault();
                if (!isFirstTrackOfQueue)
                {
                    if (!CurrentTrack.IsStopRequested)
                    {
                        Logger.Important(
                            "Current music track is not first in the queue anymore - stop it with fadeout");
                        CurrentTrack.RequestStop();
                    }
                }

                if (ComponentMusicSource.State != SoundEmitterState.Stopped)
                {
                    CurrentTrack.Update();
                    return;
                }

                CurrentTrack = null;
                if (isFirstTrackOfQueue)
                {
                    Logger.Important("Current music track removed from queue: " + Queue[0].MusicResource);
                    Queue.RemoveAt(0);
                }
            }

            if (Queue.Count == 0)
            {
                // no more tracks to play
                if (currentPlaylist is null)
                {
                    // and no playlist
                    return;
                }

                SelectNextPlaylistTrackIndex();

                Queue.Add(currentPlaylist.Tracks[currentPlaylistTrackIndex]);
                Logger.Important("Music scheduled next track: " + Queue[Queue.Count - 1]);
                delayToNextTrackRemains = currentPlaylist.PauseBetweenTracksSeconds;

                if (delayToNextTrackRemains > 0)
                {
                    Logger.Important($"Music setting pause to next track: {delayToNextTrackRemains:F1} seconds");
                }

                return;
            }

            if (delayToNextTrackRemains > 0)
            {
                // wait the delay duration
                return;
            }

            // play next track
            var nextTrack = Queue[0];
            CurrentTrack = new CurrentMusicTrack(nextTrack, CurrentPlaylist);
            Logger.Important("Playing music track now: " + nextTrack);
        }

        private class ClientComponentMusicSystemUpdater : ClientComponent
        {
            public ClientComponentMusicSystemUpdater()
                : base(isLateUpdateEnabled: true)
            {
            }

            public override void LateUpdate(double deltaTime)
            {
                Tick(deltaTime);
            }
        }

        private class CurrentMusicTrack
        {
            public readonly MusicTrack MusicTrack;

            public readonly ProtoPlaylist Playlist;

            private double? startAtPosition;

            private double? stopAtPosition;

            public CurrentMusicTrack(MusicTrack musicTrack, ProtoPlaylist playlist)
            {
                this.Playlist = playlist;
                this.MusicTrack = musicTrack;
                // ensure the track is changed
                ComponentMusicSource.MusicResource = MusicResource.NoMusic;
                ComponentMusicSource.MusicResource = musicTrack.MusicResource;
                ComponentMusicSource.IsLooped = musicTrack.IsLooped;
                this.CurrentFadeInDuration = musicTrack.FadeInDuration;
                this.CurrentFadeOutDuration = musicTrack.FadeOutDuration;

                if (MusicTrackLastStopTimeManager.TryGetLastStopTime(musicTrack.MusicResource, out var lastStopTime))
                {
                    this.startAtPosition = lastStopTime;
                    ComponentMusicSource.Seek(lastStopTime);
                    Logger.Important($"Resume music {musicTrack.MusicResource} from {lastStopTime}");
                }
                else
                {
                    this.startAtPosition = null;
                }

                this.Update();
                ComponentMusicSource.Play();
            }

            public double CurrentFadeInDuration { get; set; }

            public double CurrentFadeOutDuration { get; set; }

            public bool IsStopRequested => this.stopAtPosition.HasValue;

            public void RequestStop()
            {
                if (this.IsStopRequested)
                {
                    // stop
                    return;
                }

                if (!ComponentMusicSource.IsReady)
                {
                    // music resource is not loaded and stop requested - simply stop right now
                    ComponentMusicSource.IsLooped = false;
                    ComponentMusicSource.Stop();
                    return;
                }

                if (this.Playlist != CurrentPlaylist)
                {
                    // playlist changed, in that case a special fade out duration applies
                    this.CurrentFadeOutDuration = Math.Min(this.CurrentFadeOutDuration,
                                                           this.Playlist.FadeOutDurationOnPlaylistChange);
                }

                this.stopAtPosition = ComponentMusicSource.Position + this.CurrentFadeOutDuration;
                if (this.stopAtPosition > ComponentMusicSource.Duration)
                {
                    // clamp to duration
                    this.stopAtPosition = ComponentMusicSource.Duration;
                }

                ComponentMusicSource.IsLooped = false;
            }

            public void Update()
            {
                if (!ComponentMusicSource.IsReady)
                {
                    ComponentMusicSource.Volume = 0;
                    return;
                }

                if (this.IsStopRequested
                    && ComponentMusicSource.Position > (this.stopAtPosition ?? ComponentMusicSource.Duration))
                {
                    if (this.stopAtPosition.HasValue)
                    {
                        if (this.MusicTrack.IsLooped
                            // If track is not looped, check whether there is enough track time remains
                            // to continue playing the track on the resume.
                            // Enough time: at least 10 seconds plus track fade in and out durations.
                            || ((ComponentMusicSource.Duration - this.stopAtPosition.Value)
                                > (10
                                   + this.MusicTrack.FadeInDuration
                                   + this.MusicTrack.FadeOutDuration)))
                        {
                            Logger.Important(
                                $"Stop music and remember stop time {this.MusicTrack.MusicResource} at {this.stopAtPosition.Value}");
                            MusicPlaylistLastStopTimeManager.RememberLastTrack(this.Playlist,
                                                                               this.MusicTrack);
                            MusicTrackLastStopTimeManager.RememberLastTrack(this.MusicTrack.MusicResource,
                                                                            this.stopAtPosition.Value);
                        }
                    }

                    ComponentMusicSource.Stop();

                    return;
                }

                this.ApplyFade();
            }

            private void ApplyFade()
            {
                if (ComponentMusicSource.State != SoundEmitterState.Playing)
                {
                    ComponentMusicSource.Volume = 0;
                    return;
                }

                var positionForFader = ComponentMusicSource.Position;
                if (this.startAtPosition.HasValue)
                {
                    positionForFader -= this.startAtPosition.Value;

                    if (positionForFader < 0)
                    {
                        // it's possible as track position is not accurate
                        // (due to FMOD rounding of milliseconds or multi-threading?)
                        positionForFader = 0;
                    }
                    else if (positionForFader > this.CurrentFadeInDuration)
                    {
                        // fade in completed
                        this.startAtPosition = null;
                    }
                }

                // ReSharper disable once PossibleInvalidOperationException
                var duration = this.stopAtPosition ?? ComponentMusicSource.Duration.Value;

                var volume = this.MusicTrack.Volume;

                if (positionForFader < this.CurrentFadeInDuration)
                {
                    // apply fade in
                    volume *= positionForFader / this.CurrentFadeInDuration;
                }

                if (!ComponentMusicSource.IsLooped
                    && positionForFader >= duration - this.CurrentFadeOutDuration)
                {
                    // apply fade out
                    volume *= (duration - positionForFader) / this.CurrentFadeOutDuration;
                    if (volume < 0)
                    {
                        // clamp volume to 0 (it's possible when position > duration)
                        volume = 0;
                    }
                }

                ComponentMusicSource.Volume = (float)volume;
            }
        }
    }
}