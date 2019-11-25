namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System;

    /// <summary>
    /// Configure angles offset for weapon spread.
    /// </summary>
    public readonly struct WeaponFirePatternPreset
    {
        public readonly double[] CycledSequence;

        public readonly double[] InitialSequence;

        /// <param name="initialSequence">Array of angle offsets to use initially.</param>
        /// <param name="cycledSequence">Array of angle offsets to use when initial sequence finished.</param>
        public WeaponFirePatternPreset(
            double[] initialSequence,
            double[] cycledSequence)
        {
            if ((initialSequence is null
                 && !(cycledSequence is null))
                || (!(initialSequence is null)
                    && cycledSequence is null))
            {
                throw new Exception("One of the arrays is null while the other is not null");
            }

            if (initialSequence is null)
            {
                this.InitialSequence = this.CycledSequence = null;
                return;
            }

            if (initialSequence.Length == 0
                || cycledSequence.Length == 0)
            {
                throw new Exception("Incorrect weapon fire spreads - arrays cannot be empty");
            }

            this.InitialSequence = initialSequence;
            this.CycledSequence = cycledSequence;
        }

        public bool IsEnabled => !(this.InitialSequence is null);
    }
}