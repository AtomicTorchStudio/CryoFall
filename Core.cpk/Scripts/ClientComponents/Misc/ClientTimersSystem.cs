// ReSharper disable once CheckNamespace

namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ClientTimersSystem : ClientComponent
    {
        public static readonly ICoreClientService Core = Client.Core;

        private static ClientTimersSystem instance;

        private readonly SortedList<double, Action> sortedActionsList = new SortedList<double, Action>();

        private readonly List<(double time, Action action)> tempActionsList = new List<(double time, Action action)>();

        private bool isEnumerating;

        public static void AddAction(double delaySeconds, Action action)
        {
            Api.ValidateIsClient();
            delaySeconds = Math.Max(0, delaySeconds);
            var timeToInvokeAt = delaySeconds + Core.ClientRealTime;
            GetOrCreateInstance().AddActionInternal(timeToInvokeAt, action);
        }

        public override void Update(double deltaTime)
        {
            if (this.sortedActionsList.Count == 0)
            {
                // no delayed actions
                return;
            }

            var currentTime = Core.ClientRealTime;

            var countToRemove = 0;
            this.isEnumerating = true;

            foreach (var pair in this.sortedActionsList)
            {
                var time = pair.Key;
                if (time > currentTime)
                {
                    // cannot remove this or any next entry (sorted list!)
                    break;
                }

                countToRemove++;
                var action = pair.Value;
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "Exception during execution of the delayed action.");
                }
            }

            // remove executed actions
            for (var i = 0; i < countToRemove; i++)
            {
                this.sortedActionsList.RemoveAt(0);
            }

            this.isEnumerating = false;

            if (this.tempActionsList.Count == 0)
            {
                return;
            }

            // add entries scheduled to add during this method execution
            foreach (var entry in this.tempActionsList)
            {
                this.AddActionInternal(entry.time, entry.action);
            }

            this.tempActionsList.Clear();
        }

        protected static ClientTimersSystem GetOrCreateInstance()
        {
            return instance ??= Client.Scene
                                      .CreateSceneObject("TimersManager")
                                      .AddComponent<ClientTimersSystem>();
        }

        protected override void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        protected override void OnEnable()
        {
            if (instance != null)
            {
                throw new Exception("Instance already set");
            }

            instance = this;
        }

        private void AddActionInternal(double time, Action action)
        {
            if (this.isEnumerating)
            {
                this.tempActionsList.Add((time, action));
                return;
            }

            if (this.sortedActionsList.TryGetValue(time, out var alreadyStoredAction))
            {
                // append delegate (they're will be executed in chain)
                alreadyStoredAction += action;
                this.sortedActionsList[time] = alreadyStoredAction;
                return;
            }

            this.sortedActionsList.Add(time, action);
        }
    }
}