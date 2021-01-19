namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SoundResourceSet
    {
        private readonly List<ValueWithWeight<SoundResource>> sounds;

        public SoundResourceSet()
        {
            this.sounds = new List<ValueWithWeight<SoundResource>>();
        }

        public SoundResourceSet(List<ValueWithWeight<SoundResource>> sounds)
        {
            this.sounds = sounds;
        }

        public SoundResourceSet Add(SoundResource soundResource, double weight = 1)
        {
            if (soundResource is null)
            {
                throw new Exception("Sound resource is not provided");
            }

            this.sounds.Add(new ValueWithWeight<SoundResource>(soundResource, weight));
            return this;
        }

        public SoundResourceSet Add(string localSoundFilePath, double weight = 1)
        {
            localSoundFilePath = localSoundFilePath.TrimEnd('*');
            using var tempFilesList = Api.Shared.FindFilesWithTrailingNumbers(
                ContentPaths.Sounds + localSoundFilePath);
            if (tempFilesList.Count == 0)
            {
                // no sounds found - add "placeholder" file
                return this.Add(new SoundResource(localSoundFilePath), weight);
            }

            foreach (var file in tempFilesList.AsList())
            {
                this.sounds.Add(new ValueWithWeight<SoundResource>(new SoundResource(file), weight));
            }

            return this;
        }

        public SoundResourceSet Clone()
        {
            return new(new List<ValueWithWeight<SoundResource>>(this.sounds));
        }

        public ReadOnlySoundResourceSet ToReadOnly()
        {
            return new(this.sounds);
        }
    }
}