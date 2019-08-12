namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.DataStructures;

    public static class ClientSoundRepetitionProtectionManager
    {
        private const double ClientTimerIntervalSeconds = 1.667;

        private const double EntryLifetimeSeconds = 5.0;

        private static readonly ICoreClientService ClientCore = Api.IsClient ? Api.Client.Core : null;

        private static readonly List<Entry> List = new List<Entry>(capacity: 8);

        static ClientSoundRepetitionProtectionManager()
        {
            if (Api.IsClient)
            {
                ClientTimersSystem.AddAction(ClientTimerIntervalSeconds, ClientTimerCallback);
            }
        }

        public static SoundResource ClientGetSound(
            ArrayWithWeights<SoundResource> soundsSet,
            object soundPreset,
            object repetitionProtectionKey)
        {
            Api.ValidateIsClient();

            Entry entry = null;
            var key = new Key(soundsSet, soundPreset, repetitionProtectionKey);

            foreach (var e in List)
            {
                if (e.Key == key)
                {
                    // found entry
                    entry = e;
                    break;
                }
            }

            if (entry == null)
            {
                // no entry found - create new
                entry = new Entry(key, CalculateMaxRepeatCount(soundsSet));
                List.Add(entry);
            }

            return entry.GetSound(soundsSet);
        }

        private static int CalculateMaxRepeatCount(ArrayWithWeights<SoundResource> soundsSet)
        {
            var result = soundsSet.Count / 2;
            return result > 2 ? 2 : result;
        }

        private static void ClientTimerCallback()
        {
            // schedule next callback
            ClientTimersSystem.AddAction(ClientTimerIntervalSeconds, ClientTimerCallback);

            // calculate expiration time
            var time = ClientCore.ClientRealTime - EntryLifetimeSeconds;

            for (var index = 0; index < List.Count; index++)
            {
                var entry = List[index];
                if (time >= entry.LastAccessedTime)
                {
                    // entry outdated, remove it
                    List.RemoveAt(index);
                    index--;
                }
            }
        }

        private struct Key : IEquatable<Key>
        {
            private readonly object repetitionProtectionKey;

            private readonly object soundPreset;

            private readonly object soundsSet;

            public Key(object soundsSet, object soundPreset, object repetitionProtectionKey)
            {
                this.repetitionProtectionKey = repetitionProtectionKey;
                this.soundPreset = soundPreset;
                this.soundsSet = soundsSet;
            }

            public static bool operator ==(Key left, Key right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Key left, Key right)
            {
                return !left.Equals(right);
            }

            public bool Equals(Key other)
            {
                return Equals(this.soundPreset,                other.soundPreset)
                       && Equals(this.repetitionProtectionKey, other.repetitionProtectionKey)
                       && Equals(this.soundsSet,               other.soundsSet);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is Key key
                       && this.Equals(key);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = this.repetitionProtectionKey != null
                                       ? this.repetitionProtectionKey.GetHashCode()
                                       : 0;
                    hashCode = (hashCode * 397) ^ (this.soundPreset != null ? this.soundPreset.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (this.soundsSet != null ? this.soundsSet.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        private class Entry
        {
            public readonly Key Key;

            private readonly SoundResource[] history;

            private int nextHistoryEntryIndex;

            public Entry(Key key, int maxRepeatCount)
            {
                this.Key = key;
                this.history = new SoundResource[maxRepeatCount];

                if (maxRepeatCount == 0)
                {
                    throw new Exception("Should never happen");
                }
            }

            public double LastAccessedTime { get; private set; }

            public SoundResource GetSound(ArrayWithWeights<SoundResource> soundsSet)
            {
                this.LastAccessedTime = ClientCore.ClientRealTime;

                var historySize = this.history.Length;

                // let's get not repeated sound entry
                while (true)
                {
                    var randomSoundResource = soundsSet.GetSingleRandomElement();
                    var isFoundInHistory = false;

                    for (var index = 0; index < historySize; index++)
                    {
                        if (randomSoundResource.Equals(this.history[index]))
                        {
                            isFoundInHistory = true;
                            break;
                        }
                    }

                    if (isFoundInHistory)
                    {
                        // continue search
                        continue;
                    }

                    // found not yet played sound resource
                    this.history[this.nextHistoryEntryIndex] = randomSoundResource;

                    this.nextHistoryEntryIndex++;
                    if (this.nextHistoryEntryIndex == historySize)
                    {
                        // recycle history from beginning
                        this.nextHistoryEntryIndex = 0;
                    }

                    return randomSoundResource;
                }
            }
        }
    }
}