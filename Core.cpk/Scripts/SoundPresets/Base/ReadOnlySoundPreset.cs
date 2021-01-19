namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public sealed class ReadOnlySoundPreset<TSoundKey>
        where TSoundKey : struct, Enum
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ICurrentGameService ClientGame = Api.IsClient ? Api.Client.CurrentGame : null;

        // ReSharper disable once StaticMemberInGenericType
        private static ulong lastPlayedWithLimitFrameNumber;

        public readonly (float min, float max)? CustomDistance;

        public readonly (float min, float max)? CustomDistance3DSpread;

        private readonly Dictionary<TSoundKey, ReadOnlySoundResourceSet> dictionary;

        private ReadOnlySoundPreset(
            [NotNull] Dictionary<TSoundKey, ReadOnlySoundResourceSet> dictionary,
            (float min, float max)? customDistance,
            (float min, float max)? customDistance3DSpread)
        {
            this.dictionary = dictionary;
            this.CustomDistance = customDistance;
            this.CustomDistance3DSpread = customDistance3DSpread;
        }

        public int SoundsCount
            => this.dictionary.Sum(v => v.Value.Count);

        public void ApplyCustomDistance(IComponentSoundEmitter emitter)
        {
            if (this.CustomDistance.HasValue)
            {
                emitter.CustomMinDistance = this.CustomDistance.Value.min;
                emitter.CustomMaxDistance = this.CustomDistance.Value.max;
            }

            if (this.CustomDistance3DSpread.HasValue)
            {
                emitter.CustomMinDistance3DSpread = this.CustomDistance3DSpread.Value.min;
                emitter.CustomMaxDistance3DSpread = this.CustomDistance3DSpread.Value.max;
            }
        }

        /// <summary>
        /// Create editable sound preset.
        /// </summary>
        public SoundPreset<TSoundKey> Clone()
        {
            return new(
                this.dictionary.ToDictionary(p => p.Key, p => p.Value.Clone()),
                this.CustomDistance,
                this.CustomDistance3DSpread);
        }

        /// <summary>
        /// Please note - this playlist might have custom play distance. Take this into account if you want to play the sound you
        /// got.
        /// </summary>
        public SoundResource GetSound(TSoundKey key, object repetitionProtectionKey = null)
        {
            var set = this.dictionary.Find(key);
            if (set is null)
            {
                return SoundResource.NoSound;
            }

            return set.GetSound(this, repetitionProtectionKey);
        }

        public bool HasSound(TSoundKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public bool PlaySound(
            TSoundKey key,
            IWorldObject worldObject,
            float volume = 1,
            float pitch = 1)
        {
            return this.PlaySound(key, worldObject, out _, volume, pitch);
        }

        public bool PlaySound(
            TSoundKey key,
            IWorldObject worldObject,
            out IComponentSoundEmitter emitter,
            float volume = 1,
            float pitch = 1)
        {
            if (Api.IsServer)
            {
                Api.Logger.Error("Cannot play sound on the server");
                emitter = null;
                return false;
            }

            var soundResource = this.GetSound(key, repetitionProtectionKey: worldObject);
            this.WriteLog($"Play sound: {key} (\"{soundResource}\") at {worldObject}");
            if (soundResource == SoundResource.NoSound)
            {
                Api.ValidateIsClient();
                //Api.Logger.WriteError("No sound set for: " + key);
                emitter = null;
                return false;
            }

            emitter = Api.Client.Audio.PlayOneShot(soundResource,
                                                   worldObject,
                                                   volume: volume,
                                                   pitch: pitch);
            this.ApplyCustomDistance(emitter);
            return true;
        }

        public bool PlaySound(
            TSoundKey key,
            IProtoWorldObject protoWorldObject,
            Vector2D worldPosition,
            float volume = 1,
            float pitch = 1)
        {
            return this.PlaySound(key: key,
                                  protoWorldObject: protoWorldObject,
                                  emitter: out _,
                                  worldPosition: worldPosition,
                                  volume: volume,
                                  pitch: pitch);
        }

        public bool PlaySound(
            TSoundKey key,
            IProtoWorldObject protoWorldObject,
            Vector2D worldPosition,
            out IComponentSoundEmitter emitter,
            float volume = 1,
            float pitch = 1)
        {
            var soundResource = this.GetSound(key, repetitionProtectionKey: protoWorldObject);
            this.WriteLog($"Play sound: {key} (\"{soundResource}\") for {protoWorldObject} at {worldPosition}");
            if (soundResource == SoundResource.NoSound)
            {
                Api.ValidateIsClient();
                emitter = null;
                return false;
            }

            // apply world position offset accordingly to the object type
            if (protoWorldObject is IProtoStaticWorldObject protoStaticWorldObject)
            {
                worldPosition += protoStaticWorldObject.Layout.Center;
            }
            else if (protoWorldObject is IProtoCharacter protoCharacter)
            {
                worldPosition += (0, protoCharacter.CharacterWorldHeight);
            }

            emitter = Api.Client.Audio.PlayOneShot(soundResource,
                                                   worldPosition,
                                                   volume: volume,
                                                   pitch: pitch);
            this.ApplyCustomDistance(emitter);
            return true;
        }

        /// <summary>
        /// Play sound 2D (without 3D positioning in the game world). Can apply limit.
        /// </summary>
        /// <param name="key">Sound key.</param>
        /// <param name="volume"></param>
        /// <param name="limitOnePerFrame">
        /// Determines whether the limit for playing 2D sound should be applied (when no world position is specified). If true,
        /// only one sound per frame will be allowed to start playing. Subsequent calls of this methods will be suppressed until
        /// the next frame.
        /// The limit is applied to all presets of this type.
        /// </param>
        public bool PlaySound(
            TSoundKey key,
            float volume = 1,
            float pitch = 1,
            bool limitOnePerFrame = true)
        {
            var soundResource = this.GetSound(key);
            this.WriteLog($"Play sound: {key} (\"{soundResource}\") (2D)");
            if (soundResource is null)
            {
                Api.ValidateIsClient();
                return false;
            }

            if (limitOnePerFrame
                && CheckLimit())
            {
                // limited, but sound is present
                return true;
            }

            var emitter = Api.Client.Audio.PlayOneShot(soundResource,
                                                       volume: volume,
                                                       pitch: pitch);
            this.ApplyCustomDistance(emitter);
            return true;
        }

        public bool PlaySound(
            TSoundKey key,
            Vector2D worldPosition,
            float volume = 1,
            float pitch = 1)
        {
            return this.PlaySound(key: key,
                                  worldPosition: worldPosition,
                                  emitter: out _,
                                  volume: volume,
                                  pitch: pitch);
        }

        public bool PlaySound(
            TSoundKey key,
            Vector2D worldPosition,
            out IComponentSoundEmitter emitter,
            float volume = 1,
            float pitch = 1)
        {
            var soundResource = this.GetSound(key);
            this.WriteLog($"Play sound: {key} (\"{soundResource}\") at {worldPosition}");
            if (soundResource == SoundResource.NoSound)
            {
                Api.ValidateIsClient();
                emitter = null;
                return false;
            }

            emitter = Api.Client.Audio.PlayOneShot(soundResource,
                                                   worldPosition,
                                                   volume: volume,
                                                   pitch: pitch);
            this.ApplyCustomDistance(emitter);
            return true;
        }

        internal static ReadOnlySoundPreset<TSoundKey> Create(
            Dictionary<TSoundKey, ReadOnlySoundResourceSet> dictionary,
            (float min, float max)? customDistance,
            (float min, float max)? customDistance3DSpread)
        {
            return new(dictionary,
                       customDistance,
                       customDistance3DSpread);
        }

        /// <summary>
        /// Play sound limited to one sound per frame (applies only to 2D sounds).
        /// </summary>
        private static bool CheckLimit()
        {
            var currentFrameNumber = ClientGame.ServerFrameNumber;
            if (currentFrameNumber == lastPlayedWithLimitFrameNumber)
            {
                // limit playing
                return true;
            }

            lastPlayedWithLimitFrameNumber = currentFrameNumber;
            return false;
        }

        [Conditional("AUDIO_PRESETS_LOG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteLog(string message)
        {
            Api.Logger.Dev("[AUDIO] [PRESET] " + message);
        }
    }
}