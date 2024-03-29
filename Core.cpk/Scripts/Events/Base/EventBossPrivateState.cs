﻿namespace AtomicTorch.CBND.CoreMod.Events
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class EventBossPrivateState : BasePrivateState
    {
        public bool IsSpawnCompleted { get; set; }

        public List<IWorldObject> SpawnedWorldObjects { get; } = new();

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