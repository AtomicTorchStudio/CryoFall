namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class SoundConstants
    {
        /// <summary>
        /// Max distance to the ambient sound.
        /// Lower distance means a quicker change of ambient sounds as player moves between the different tile prototypes.
        /// Please note: if you want to change this value, please ensure this value is even!
        /// </summary>
        public const int AmbientMaxDistance = 8;

        /// <summary>
        /// Max distance of 3D sounds for spread  (in world units, 1 unit == 1 grid tile).
        /// Outside this distance objects will sound only from left or right channel (relative to audio listener).
        /// If the audio listener is located at distance==0, the sound will be heard from both channels.
        /// Please note: can be overridden by <see cref="IComponentSoundEmitter" />.
        /// </summary>
        public const float AudioListener3DSpreadMaxDistance = 15f;

        /// <summary>
        /// Same as AudioListener3DSpreadMaxDistance but if object is closer than this distance
        /// it will be heard from both channels completely (i.e. it defines a dead zone).
        /// Please note: can be overridden by <see cref="IComponentSoundEmitter" />.
        /// </summary>
        public const float AudioListener3DSpreadMinDistance = 5f;

        /// <summary>
        /// Max distance (in world units, 1 unit == 1 grid tile).
        /// Objects located with distance larger than this will be not audible.
        /// Please note: can be overridden by <see cref="IComponentSoundEmitter" />.
        /// </summary>
        public const float AudioListenerMaxDistance = 15;

        public const float AudioListenerMaxDistanceObjectDestroy = 20;

        public const float AudioListenerMaxDistanceObjectHit = 20;

        public const float AudioListenerMaxDistanceRangedShotFirearms = 45;

        public const float AudioListenerMaxDistanceRangedShotBows = 15;

        public const float AudioListenerMaxDistanceRangedShotMobs = 15;

        /// <summary>
        /// Same as AudioListenerMinDistance but if object is closer than this distance
        /// it will be played at full volume (i.e. it defines a dead zone).
        /// Please note: can be overridden by <see cref="IComponentSoundEmitter" />.
        /// </summary>
        public const float AudioListenerMinDistance = 1f;

        public const float AudioListenerMinDistanceObjectDestroy = 3;

        public const float AudioListenerMinDistanceObjectHit = 3;

        public const float AudioListenerMinDistanceRangedShot = 6;

        /// <summary>
        /// Enable or disable the audio log - very useful when you need to determine what sounds are played.
        /// </summary>
        public const bool IsAudioLogEnabled = false;

        /// <summary>
        /// Volume for all destroy sounds.
        /// </summary>
        public const float VolumeDestroy = 0.5f;

        /// <summary>
        /// Volume for character footsteps.
        /// </summary>
        public const float VolumeFootstepsMultiplier = 0.5f;

        /// <summary>
        /// Volume for all hit/damage sounds.
        /// </summary>
        public const float VolumeHit = 1.0f;

        public const float VolumeUIChat = 0.8f;

        public const float VolumeUINotifications = 0.5f;

        /// <summary>
        /// Volume for all weapon-related sounds (except hit).
        /// </summary>
        public const float VolumeWeapon = 0.4f;

        /// <summary>
        /// Applies the constants. Please modify only the constants defined above and don't touch this method.
        /// This method should be only called once (by <see cref="BootstrapperClientCore" />).
        /// </summary>
        internal static void ApplyConstants()
        {
            var audio = Api.Client.Audio;
            audio.IsAudioLogEnabled = IsAudioLogEnabled;

            audio.AudioListenerMinDistance = AudioListenerMinDistance;
            audio.AudioListenerMaxDistance = AudioListenerMaxDistance;

            audio.AudioListener3DSpreadMinDistance = AudioListener3DSpreadMinDistance;
            audio.AudioListener3DSpreadMaxDistance = AudioListener3DSpreadMaxDistance;
        }
    }
}