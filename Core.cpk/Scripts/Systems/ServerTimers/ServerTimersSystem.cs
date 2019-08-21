namespace AtomicTorch.CBND.CoreMod.Systems.ServerTimers
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ServerTimersSystem : ProtoSystem<ServerTimersSystem>
    {
        private const string DatabaseKeyForLogicObjectSingleton = "LogicObjectSingleton";

        public static readonly IGameServerService Game = Api.IsServer
                                                             ? Api.Server.Game
                                                             : null;

        private readonly SortedList<double, Action> sortedActionsList = new SortedList<double, Action>();

        private readonly List<(double time, Action action)> tempActionsList = new List<(double time, Action action)>();

        private bool isEnumerating;

        public override string Name => "Server timers system";

        public static void AddAction(double delaySeconds, Action action)
        {
            Api.ValidateIsServer();
            var timeToInvokeAt = delaySeconds + Game.FrameTime;
            Instance.AddActionInternal(timeToInvokeAt, action);
        }

        public static double SharedGetTimeRemainingFraction(
            double timeEnds,
            double duration,
            out double timeRemainingSeconds)
        {
            if (duration <= 0)
            {
                timeRemainingSeconds = 0;
                return 0;
            }

            if (duration >= double.MaxValue)
            {
                // infinite
                timeRemainingSeconds = double.MaxValue;
                return 1.0;
            }

            timeRemainingSeconds = SharedGetTimeRemainingSeconds(timeEnds);
            timeRemainingSeconds = MathHelper.Clamp(timeRemainingSeconds, 0, duration);
            return timeRemainingSeconds / duration;
        }

        public static double SharedGetTimeRemainingSeconds(double serverTime)
        {
            if (serverTime == double.MaxValue)
            {
                return serverTime;
            }

            var delta = serverTime
                        - (Api.IsServer
                               ? Api.Server.Game.FrameTime
                               : BaseViewModel.Client.CurrentGame.ServerFrameTimeApproximated);
            if (delta < 0)
            {
                delta = 0;
            }

            return delta;
        }

        protected override void PrepareSystem()
        {
            base.PrepareSystem();
            if (IsServer)
            {
                this.ServerLoadSystem();
                Server.World.WorldBoundsChanged += this.ServerWorldBoundsChangedHandler;
            }
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

        private void ServerLoadSystem()
        {
            if (!Server.Database.TryGet<ILogicObject>(nameof(ServerTimersSystem),
                                                      DatabaseKeyForLogicObjectSingleton,
                                                      out var logicObject))
            {
                // create logic object to keep updating this system
                logicObject = Server.World.CreateLogicObject<ServerTimersSystemUpdater>();
                Server.Database.Set(nameof(ServerTimersSystem), DatabaseKeyForLogicObjectSingleton, logicObject);
            }
        }

        private void ServerUpdate()
        {
            if (this.sortedActionsList.Count == 0)
            {
                // no delayed actions
                return;
            }

            var currentTime = Game.FrameTime;

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

        private void ServerWorldBoundsChangedHandler()
        {
            Server.Database.Remove(nameof(ServerTimersSystem), DatabaseKeyForLogicObjectSingleton);
            this.ServerLoadSystem();
        }

        private class ServerTimersSystemUpdater
            : ProtoGameObject<ILogicObject, EmptyPrivateState, EmptyPublicState, EmptyClientState>,
              IProtoLogicObject
        {
            public override double ClientUpdateIntervalSeconds => double.MaxValue; // never

            public override string Name => "Logic object to update Server timers system";

            public override double ServerUpdateIntervalSeconds => 0; // every frame

            protected override void ServerUpdate(ServerUpdateData data)
            {
                Instance.ServerUpdate();
            }
        }
    }
}