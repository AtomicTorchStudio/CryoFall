namespace AtomicTorch.CBND.CoreMod.Systems.ServerTimers
{
    using System;
    using System.Collections.Generic;
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

        private readonly SortedList<double, Action> sortedActionsList
            = IsServer
                  ? new SortedList<double, Action>(capacity: 256)
                  : null;

        private readonly List<Action> tempExecutionList
            = IsServer
                  ? new List<Action>(capacity: 64)
                  : null;

        private readonly List<(double time, Action action)> tempScheduledActionsList
            = IsServer
                  ? new List<(double time, Action action)>(capacity: 64)
                  : null;

        private bool isEnumerating;

        public override string Name => "Server timers system";

        public static void AddAction(double delaySeconds, Action action)
        {
            Api.ValidateIsServer();
            delaySeconds = Math.Max(0, delaySeconds);
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
                               : Api.Client.CurrentGame.ServerFrameTimeApproximated);
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
                this.tempScheduledActionsList.Add((time, action));
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

            this.isEnumerating = true;

            // select actions to execute
            foreach (var pair in this.sortedActionsList)
            {
                var time = pair.Key;
                if (time > currentTime)
                {
                    // this and following actions should be executed later so we stop here
                    break;
                }

                this.tempExecutionList.Add(pair.Value);
            }

            if (this.tempExecutionList.Count > 0)
            {
                // remove and execute the selected actions
                for (var i = 0; i < this.tempExecutionList.Count; i++)
                {
                    this.sortedActionsList.RemoveAt(0);
                }

                foreach (var action in this.tempExecutionList)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception(ex, "Exception during execution of the delayed action.");
                    }
                }

                this.tempExecutionList.Clear();
            }

            this.isEnumerating = false;

            if (this.tempScheduledActionsList.Count > 0)
            {
                // add entries scheduled to add during this method execution
                foreach (var entry in this.tempScheduledActionsList)
                {
                    this.AddActionInternal(entry.time, entry.action);
                }

                this.tempScheduledActionsList.Clear();
            }
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