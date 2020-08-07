namespace AtomicTorch.CBND.CoreMod.Events.Base
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.ActiveEventsSystem;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    [PrepareOrder(afterType: typeof(ProtoTrigger))]
    [PrepareOrder(afterType: typeof(IProtoZone))]
    public abstract class ProtoEvent
        <TEventPrivateState,
         TEventPublicState,
         TEventClientState>
        : ProtoGameObject<ILogicObject,
              TEventPrivateState,
              TEventPublicState,
              TEventClientState>,
          IProtoLogicObject, IProtoEvent
        where TEventPrivateState : BasePrivateState, new()
        where TEventPublicState : EventPublicState, new()
        where TEventClientState : BaseClientState, new()
    {
        protected ProtoEvent()
        {
            var name = this.GetType().Name;
            if (name.StartsWith("Event"))
            {
                name = name.Substring("Event".Length);
            }

            this.ShortId = name;
            this.Icon = new TextureResource($"Events/{name}.png");
        }

        public abstract ushort AreaRadius { get; }

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public abstract bool ConsolidateNotifications { get; }

        public abstract string Description { get; }

        public abstract TimeSpan EventDuration { get; }

        public virtual TimeSpan EventStartPostponeDurationFrom { get; }
            = TimeSpan.FromMinutes(30);

        public virtual TimeSpan EventStartPostponeDurationTo { get; }
            = TimeSpan.FromMinutes(60);

        public virtual ITextureResource Icon { get; }

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override string ShortId { get; }

        public IReadOnlyList<BaseTriggerConfig> Triggers { get; private set; }

        public virtual bool ServerIsTriggerAllowed(ProtoTrigger trigger)
        {
            return true;
        }

        public sealed override void ServerOnDestroy(ILogicObject gameObject)
        {
            ActiveEventsSystem.ServerUnregisterEvent(gameObject);
            this.ServerOnEventDestroyed(gameObject);
            Logger.Important("Event destroyed: " + gameObject);
        }

        public abstract string SharedGetProgressText(ILogicObject activeEvent);

        void IProtoEvent.ServerForceCreateAndStart()
        {
            this.ServerOnEventStartRequested(null);
        }

        protected static TProtoTrigger GetTrigger<TProtoTrigger>()
            where TProtoTrigger : ProtoTrigger, new()
        {
            return Api.GetProtoEntity<TProtoTrigger>();
        }

        protected static Vector2Ushort SharedSelectRandomPositionInsideTheCircle(
            Vector2Ushort circlePosition,
            ushort circleRadius)
        {
            var offset = circleRadius * RandomHelper.NextDouble();
            var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
            return new Vector2Ushort((ushort)(circlePosition.X + offset * Math.Cos(angle)),
                                     (ushort)(circlePosition.Y + offset * Math.Sin(angle)));
        }

        protected sealed override void PrepareProto()
        {
            if (IsClient)
            {
                return;
            }

            var triggers = new Triggers();
            this.ServerPrepareEvent(triggers);
            this.Triggers = triggers.ToReadOnly();

            foreach (var triggerConfig in this.Triggers)
            {
                triggerConfig.ServerRegister(
                    callback: () => this.ServerEventTriggerCallback(triggerConfig),
                    $"Event.{this.GetType().Name}");
            }

            Server.World.WorldBoundsChanged += this.ServerWorldChangedHandler;
            this.ServerWorldChangedHandler();
        }

        protected bool ServerCreateAndStartEventInstance()
        {
            var activeEvent = Server.World.CreateLogicObject(this);
            Logger.Important("Event created: " + activeEvent);

            try
            {
                this.ServerOnEventStarted(activeEvent);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Error when starting an event. The event will be destroyed: " + activeEvent);
                Server.World.DestroyObject(activeEvent);
            }

            return false;
        }

        protected (TimeSpan from, TimeSpan to) ServerGetIntervalForThisEvent(
            (TimeSpan from, TimeSpan to) defaultInterval)
        {
            var rateKey = "EventInterval." + this.ShortId;
            var rateDefaultValue = defaultInterval.from != defaultInterval.to
                                       ? defaultInterval.from.TotalHours.ToString("0.0#")
                                         + "-"
                                         + defaultInterval.to.TotalHours.ToString("0.0#")
                                       : defaultInterval.from.TotalHours.ToString("0.0#");
            var rateDescription = $"{this.Name} world event interval (in hours).";

            var configIntervalString = ServerRates.Get(key: rateKey,
                                                       defaultValue: rateDefaultValue,
                                                       description: rateDescription);

            if (string.Equals(configIntervalString, rateDefaultValue, StringComparison.Ordinal))
            {
                return defaultInterval;
            }

            // try parse the interval
            try
            {
                TimeSpan from, to;

                if (configIntervalString.IndexOf("-", StringComparison.Ordinal) > 0)
                {
                    // parse range
                    var split = configIntervalString.Split('-');
                    from = TimeSpan.FromHours(double.Parse(split[0]));
                    to = TimeSpan.FromHours(double.Parse(split[1]));
                }
                else
                {
                    // parse single value
                    from = to = TimeSpan.FromHours(double.Parse(configIntervalString));
                }

                if (from.TotalSeconds <= 0
                    || to.TotalSeconds <= 0
                    || to < from)
                {
                    throw new Exception("Incorrect time interval for: " + rateKey);
                }

                return (from, to);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                ServerRates.Reset(key: rateKey,
                                  defaultValue: rateDefaultValue,
                                  description: rateDescription);
                return defaultInterval;
            }
        }

        protected sealed override void ServerInitialize(ServerInitializeData data)
        {
            var activeEvent = data.GameObject;
            var publicState = data.PublicState;

            if (data.IsFirstTimeInit)
            {
                publicState.EventEndTime = Server.Game.FrameTime
                                           + this.EventDuration.TotalSeconds;
            }

            // schedule rest of the initialization on the next frame
            ServerTimersSystem.AddAction(
                delaySeconds: 0,
                () =>
                {
                    if (activeEvent.IsDestroyed)
                    {
                        // the event was destroyed soon after creation
                        // (it was not correct one or something else)
                        return;
                    }

                    ActiveEventsSystem.ServerRegisterEvent(activeEvent);
                });

            this.ServerInitializeEvent(data);
        }

        protected abstract void ServerInitializeEvent(ServerInitializeData data);

        protected abstract void ServerOnEventDestroyed(ILogicObject activeEvent);

        protected abstract void ServerOnEventStarted(ILogicObject activeEvent);

        protected virtual void ServerOnEventStartRequested(BaseTriggerConfig triggerConfig)
        {
            this.ServerCreateAndStartEventInstance();
        }

        protected abstract void ServerPrepareEvent(Triggers triggers);

        protected IServerZone ServerSelectRandomZoneWithEvenDistribution(IReadOnlyList<(IServerZone Zone, uint Weight)> list)
        {
            if (list.Count == 0)
            {
                return null;
            }

            // let's perform a weighted pick
            // each zone gets a priority according to its number of positions (cells)
            long totalPositionsCount = 0;
            foreach (var z in list)
            {
                totalPositionsCount += z.Weight;
            }

            var value = (long)(RandomHelper.NextDouble() * totalPositionsCount);
            var accumulator = 0u;

            for (var index = 0; index < list.Count - 1; index++)
            {
                var zone = list[index];
                accumulator += zone.Weight;

                if (value < accumulator)
                {
                    return zone.Zone;
                }
            }

            return list[list.Count - 1].Zone;
        }

        protected virtual void ServerTryFinishEvent(ILogicObject activeEvent)
        {
            // destroy after a second delay
            // to ensure the public state is synchronized with the clients
            ServerTimersSystem.AddAction(
                1,
                () =>
                {
                    if (!activeEvent.IsDestroyed)
                    {
                        Server.World.DestroyObject(activeEvent);
                    }
                });
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var publicState = data.PublicState;

            var timeRemains = publicState.EventEndTime - Server.Game.FrameTime;
            if (timeRemains > 0)
            {
                return;
            }

            this.ServerTryFinishEvent(data.GameObject);
        }

        protected virtual void ServerWorldChangedHandler()
        {
        }

        private void ServerEventTriggerCallback(BaseTriggerConfig triggerConfig)
        {
            if (Api.IsEditor)
            {
                // event triggers are suppressed in Editor
                return;
            }

            if (this.ServerIsTriggerAllowed(triggerConfig.Trigger))
            {
                this.ServerOnEventStartRequested(triggerConfig);
                return;
            }

            if (!(triggerConfig.Trigger is TriggerTimeInterval triggerTimeInterval))
            {
                return;
            }

            // try to postpone the event
            double postponeDuration;
            {
                var postponeDurationFrom = this.EventStartPostponeDurationFrom.TotalSeconds;
                if (postponeDurationFrom <= 0)
                {
                    return;
                }

                var postponeDurationTo = this.EventStartPostponeDurationTo.TotalSeconds;
                if (postponeDurationTo <= postponeDurationFrom)
                {
                    postponeDurationTo = postponeDurationFrom;
                }

                postponeDuration = postponeDurationFrom
                                   + RandomHelper.NextDouble() * (postponeDurationTo - postponeDurationFrom);
            }

            triggerTimeInterval.ApplyPostpone(triggerConfig,
                                              postponeDuration);
            Logger.Important($"Event start postponed on {TimeSpan.FromSeconds(postponeDuration)} - {this}");
        }
    }
}