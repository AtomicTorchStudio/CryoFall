namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class SpawnedMobsTrackingManager
    {
        private readonly List<WeakReference<ICharacter>> spawnedList = new();

        public void Add(ICharacter mob)
        {
            this.spawnedList.Add(new WeakReference<ICharacter>(mob));
        }

        public void Clear()
        {
            this.spawnedList.Clear();
        }

        public IEnumerable<ICharacter> EnumerateAll()
        {
            for (var index = 0; index < this.spawnedList.Count; index++)
            {
                var weakReference = this.spawnedList[index];
                if (weakReference.TryGetTarget(out var character)
                    && !character.IsDestroyed)
                {
                    yield return character;

                    continue;
                }

                // weak reference is dead or character is destroyed - remove character from the spawned list
                this.spawnedList.RemoveAt(index);
                index--;
            }
        }
    }
}