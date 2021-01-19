namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public class SpawnList
    {
        private List<ObjectSpawnPreset> presets = new();

        /// <summary>
        /// Adds proto entity (or entities) to the spawn list.
        /// </summary>
        /// <typeparam name="TProtoSpawnableObject">
        /// Specify a concrete entity class or an interface. It must be inherited from
        /// <see cref="IProtoSpawnableObject" /> (all items and static objects are inherited from it).
        /// </typeparam>
        /// <param name="interval">
        /// Defines how often the objects must spawn (every X*X cells). Used to calculate the desired density of the specified
        /// objects.
        /// </param>
        /// <param name="padding">Defines the minimal distance to any other objects.</param>
        ///// <param name="iterationLimitMinFraction">
        ///// Defines how many % (minimum) of the potential population could be spawned per spawn iteration depending on how
        ///// populated the world is by this spawn preset. If the world is 100% populated a min value will be used and if it's 0%
        ///// populated a max value will be used, otherwise proportional value will be used.
        ///// Value must be in range from 0 to 1.0.
        ///// Please note: this is used only after the initial spawn.
        ///// The initial spawn is performed on the world initialization with spawning as much as possible.
        ///// </param>
        ///// <param name="iterationLimitMaxFraction">
        ///// <see cref="iterationLimitMinFraction" />
        ///// </param>
        /// <returns>Object spawn preset instance (chainable).</returns>
        public ObjectSpawnPreset CreatePreset(
            double interval,
            double padding,
            //double iterationLimitMinFraction = 0,
            //double iterationLimitMaxFraction = 0.05,
            bool useSectorDensity = true,
            int paddingToLandClaimAreas = 0,
            bool spawnAtLeastOnePerSector = false)
        {
            var preset = new ObjectSpawnPreset(
                interval,
                padding,
                iterationLimitMinFraction: 0,
                iterationLimitMaxFraction: 0.05,
                isRestrictionPreset: false,
                useSectorDensity: useSectorDensity,
                paddingToLandClaimAreas,
                spawnAtLeastOnePerSector);
            this.presets.Add(preset);
            return preset;
        }

        public ObjectSpawnPreset CreateRestrictedPreset()
        {
            var preset = new ObjectSpawnPreset(interval: 0,
                                               padding: 0,
                                               iterationLimitMinFraction: 0,
                                               iterationLimitMaxFraction: 0,
                                               isRestrictionPreset: true,
                                               useSectorDensity: false,
                                               paddingToLandClaimAreas: 0);
            this.presets.Add(preset);
            return preset;
        }

        public ObjectSpawnPreset[] ToReadOnly()
        {
            if (this.presets.Count == 0)
            {
                throw new Exception("No spawn presets defined");
            }

            this.presets.TrimExcess();
            foreach (var preset in this.presets)
            {
                preset.MakeReadOnly();
            }

            var result = this.presets.ToArray();
            this.presets = null;
            return result;
        }
    }
}