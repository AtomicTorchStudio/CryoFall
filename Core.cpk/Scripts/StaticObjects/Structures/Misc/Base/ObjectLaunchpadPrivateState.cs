namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectLaunchpadPrivateState : StructurePrivateState
    {
        /// <summary>
        /// This is an array of boolean values - each matching the respective launchpad task's completion.
        /// Please note: do not overwrite internal array values as the change will not propagate to the client.
        /// Instead assign a new array (with the modified value(s)) to this property.
        /// </summary>
        [SyncToClient]
        public bool[] TaskCompletionState { get; set; }

        public void ServerInitialize(byte tasksCount)
        {
            if (this.TaskCompletionState is not null
                && this.TaskCompletionState.Length == tasksCount)
            {
                return;
            }

            this.TaskCompletionState = new bool[tasksCount];
        }
    }
}