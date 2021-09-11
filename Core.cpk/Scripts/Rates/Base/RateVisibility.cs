namespace AtomicTorch.CBND.CoreMod.Rates
{
    public enum RateVisibility : byte
    {
        /// <summary>
        /// Always visible.
        /// </summary>
        Primary = 0,

        /// <summary>
        /// Player can click a combobox to see these rates.
        /// </summary>
        Advanced = 10,

        /// <summary>
        /// For rates that are browsable only by editing the ServerRates.config file.
        /// </summary>
        Hidden = byte.MaxValue
    }
}