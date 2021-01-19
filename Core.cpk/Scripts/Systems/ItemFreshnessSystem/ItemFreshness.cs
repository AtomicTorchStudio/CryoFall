namespace AtomicTorch.CBND.CoreMod.Systems.ItemFreshnessSystem
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum ItemFreshness : byte
    {
        /// <summary>
        /// Fresh.
        /// </summary>
        Green,

        /// <summary>
        /// Average.
        /// </summary>
        Yellow,

        /// <summary>
        /// Spoiled.
        /// </summary>
        Red
    }
}