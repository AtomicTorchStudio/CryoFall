namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Extensions;

    public sealed class SoundPreset<TSoundKey>
        where TSoundKey : struct, Enum
    {
        private readonly (float min, float max)? customDistance;

        private readonly (float min, float max)? customDistance3DSpread;

        private readonly Dictionary<TSoundKey, SoundResourceSet> dictionary;

        public SoundPreset(
            (float min, float max)? customDistance = null,
            (float min, float max)? customDistance3DSpread = null)
        {
            this.dictionary = new Dictionary<TSoundKey, SoundResourceSet>();
            this.customDistance = customDistance;
            this.customDistance3DSpread = customDistance3DSpread;
        }

        internal SoundPreset(
            Dictionary<TSoundKey, SoundResourceSet> dictionary,
            (float min, float max)? customDistance,
            (float min, float max)? customDistance3DSpread)
        {
            this.dictionary = dictionary;
            this.customDistance = customDistance;
            this.customDistance3DSpread = customDistance3DSpread;
        }

        public static implicit operator ReadOnlySoundPreset<TSoundKey>(SoundPreset<TSoundKey> preset)
        {
            return preset.ToReadOnly();
        }

        public SoundPreset<TSoundKey> Add(TSoundKey key, SoundResource soundResource, double frequency = 1)
        {
            var soundResourceSet = this.GetOrCreateSoundResourceSet(key);
            soundResourceSet.Add(soundResource, frequency);
            return this;
        }

        public SoundPreset<TSoundKey> Add(TSoundKey key, params SoundResource[] soundResources)
        {
            return this.Add(key, frequency: 1, soundResources: soundResources);
        }

        public SoundPreset<TSoundKey> Add(TSoundKey key, params string[] localSoundFilePaths)
        {
            return this.Add(key, frequency: 1, localSoundFilePaths: localSoundFilePaths);
        }

        public SoundPreset<TSoundKey> Add(TSoundKey key, double frequency, params SoundResource[] soundResources)
        {
            return this.Add(key, frequency, (IReadOnlyList<SoundResource>)soundResources);
        }

        public SoundPreset<TSoundKey> Add(TSoundKey key, string localSoundFilePath, double frequency = 1)
        {
            var soundResourceSet = this.GetOrCreateSoundResourceSet(key);
            soundResourceSet.Add(localSoundFilePath, frequency);
            return this;
        }

        public SoundPreset<TSoundKey> Add(TSoundKey key, double frequency, params string[] localSoundFilePaths)
        {
            var soundResources = localSoundFilePaths.Select(se => new SoundResource(se));
            return this.Add(key, frequency, soundResources);
        }

        public SoundPreset<TSoundKey> Add(TSoundKey key, double frequency, IEnumerable<string> localSoundFilePaths)
        {
            var soundResources = localSoundFilePaths.Select(se => new SoundResource(se));
            return this.Add(key, frequency, soundResources);
        }

        public SoundPreset<TSoundKey> Clear(TSoundKey key)
        {
            this.dictionary.Remove(key);
            return this;
        }

        public SoundPreset<TSoundKey> Clone()
        {
            return new SoundPreset<TSoundKey>(this.dictionary.ToDictionary(p => p.Key, p => p.Value.Clone()),
                                              this.customDistance,
                                              this.customDistance3DSpread);
        }

        /// <summary>
        /// This is a shortcut to calling preset.Clear(key).Add(key, "filePath").
        /// </summary>
        public SoundPreset<TSoundKey> Replace(TSoundKey key, string localSoundFilePath)
        {
            this.Clear(key);
            this.Add(key, localSoundFilePath);
            return this;
        }

        /// <summary>
        /// This is a shortcut to calling preset.Clear(key).Add(key, soundResource).
        /// </summary>
        public SoundPreset<TSoundKey> Replace(TSoundKey key, SoundResource soundResource)
        {
            this.Clear(key);
            this.Add(key, soundResource);
            return this;
        }

        public ReadOnlySoundPreset<TSoundKey> ToReadOnly()
        {
            return ReadOnlySoundPreset<TSoundKey>.Create(
                this.dictionary.ToDictionary(
                    p => p.Key,
                    p => p.Value.ToReadOnly()),
                this.customDistance,
                this.customDistance3DSpread);
        }

        private SoundPreset<TSoundKey> Add(TSoundKey key, double frequency, IEnumerable<SoundResource> soundResources)
        {
            var soundResourceSet = this.GetOrCreateSoundResourceSet(key);

            foreach (var soundResource in soundResources)
            {
                soundResourceSet.Add(soundResource, frequency);
            }

            return this;
        }

        private SoundResourceSet GetOrCreateSoundResourceSet(TSoundKey key)
        {
            if (this.dictionary.TryGetValue(key, out var result))
            {
                return result;
            }

            // create new sound resource set
            return this.dictionary[key] = new SoundResourceSet();
        }
    }

    public static class SoundPreset
    {
        /// <summary>
        /// Creates sound preset from all the files in the folder.
        /// </summary>
        /// <param name="localSoundsFolderPath">Path inside "Content/Sounds/" folder.</param>
        /// <param name="throwExceptionIfNoFilesFound"></param>
        /// <param name="customDistance">Custom sound emitter distance settings.</param>
        /// <param name="customDistance3DSpread">Custom sound emitter 3D spread distance settings.</param>
        /// <returns></returns>
        public static ReadOnlySoundPreset<TSoundKey> CreateFromFolder<TSoundKey>(
            string localSoundsFolderPath,
            bool throwExceptionIfNoFilesFound = true,
            (float min, float max)? customDistance = null,
            (float min, float max)? customDistance3DSpread = null)
            where TSoundKey : struct, Enum
        {
            if (!localSoundsFolderPath.EndsWith("/"))
            {
                localSoundsFolderPath += "/";
            }

            var preset = new SoundPreset<TSoundKey>(customDistance: customDistance,
                                                    customDistance3DSpread: customDistance3DSpread);
            foreach (var enumValue in EnumExtensions.GetValues<TSoundKey>())
            {
                preset.Add(enumValue, localSoundsFolderPath + enumValue.ToString());
            }

            var result = preset.ToReadOnly();
            if (throwExceptionIfNoFilesFound && result.SoundsCount == 0)
            {
                throw new Exception("No sounds found at " + localSoundsFolderPath);
            }

            return result;
        }
    }
}