namespace AtomicTorch.CBND.CoreMod.Events
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class EventDropPrivateState : BasePrivateState
    {
        public List<IWorldObject> SpawnedWorldObjects { get; }
            = new();

        public void Init()
        {
            for (var index = 0; index < this.SpawnedWorldObjects.Count; index++)
            {
                var worldObject = this.SpawnedWorldObjects[index];
                if (worldObject is null)
                {
                    this.SpawnedWorldObjects.RemoveAt(index--);
                }
            }
        }
    }
}