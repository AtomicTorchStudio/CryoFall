namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class LootHackingContainerPublicState : StaticObjectPublicState
    {
        /// <summary>
        /// A value from 0 to 100.
        /// </summary>
        [SyncToClient(networkDataType: typeof(float))]
        public double HackingProgressPercent { get; set; }
    }
}