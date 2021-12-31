namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.WorldEvents;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

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
            this.Icon = new TextureResource($"Events/{name}.png", qualityOffset: -1);
        }

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public abstract bool ConsolidateNotifications { get; }

        public abstract string Description { get; }

        public abstract TimeSpan EventDuration { get; }

        public virtual TimeSpan EventStartPostponeDurationFrom { get; }
            = TimeSpan.FromMinutes(5);

        public virtual TimeSpan EventStartPostponeDurationTo { get; }
            = TimeSpan.FromMinutes(15);

        public virtual ITextureResource Icon { get; }

        public virtual bool IsAvailableInCompletionist => true;

        /// <summary>
        /// Determines when this event was active last time.
        /// </summary>
        public double ServerLastActiveTime { get; private set; }

        public override double ServerUpdateIntervalSeconds => 1;

        public override string ShortId { get; }

        public IReadOnlyList<BaseTriggerConfig> Triggers { get; private set; }

        /// <summary>
        /// The event will not start on the server earlier than this delay
        /// (can be adjusted by editing event delay multiplier in the server rates config file).
        /// </summary>
        protected abstract double DelayHoursSinceWipe { get; }

        public virtual string ClientGetDescription(ILogicObject gameObject)
        {
            return this.Description;
        }

        public virtual bool ServerIsTriggerAllowed(ProtoTrigger trigger)
        {
            return true;
        }

        public sealed override void ServerOnDestroy(ILogicObject gameObject)
        {
            WorldEventsSystem.ServerUnregisterEvent(gameObject);
            this.ServerOnEventDestroyed(gameObject);
            Logger.Important("Event destroyed: " + gameObject);
        }

        public virtual string SharedGetProgressText(ILogicObject worldEvent)
        {
            return null;
        }

        void IProtoEvent.ServerForceCreateAndStart()
        {
            this.ServerOnEventStartRequested(null);
        }

        protected static TProtoTrigger GetTrigger<TProtoTrigger>()
            where TProtoTrigger : ProtoTrigger, new()
        {
            return Api.GetProtoEntity<TProtoTrigger>();
        }

        protected static bool ServerHasAnyEventOfTypeRunRecently<TProtoEvent>(TimeSpan maxTime)
            where TProtoEvent : class, IProtoEvent
        {
            var maxTimeTotalSeconds = maxTime.TotalSeconds;

            foreach (var protoEvent in Api.FindProtoEntities<TProtoEvent>())
            {
                if (Server.Game.FrameTime - protoEvent.ServerLastActiveTime
                    < maxTimeTotalSeconds)
                {
                    // this event was triggered recently
                    return true;
                }
            }

            return false;
        }

        protected sealed override void PrepareProto()
        {
            this.SharedPrepareEvent();

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
            var worldEvent = Server.World.CreateLogicObject(this);
            Logger.Important("Event created: " + worldEvent);

            try
            {
                this.ServerOnEventStarted(worldEvent);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Error when starting an event. The event will be destroyed: " + worldEvent);
                Server.World.DestroyObject(worldEvent);
            }

            return false;
        }

        protected bool ServerHasAnyEventOfType<TProtoEvent>()
            where TProtoEvent : class, IProtoEvent
        {
            using var tempEvents =
                Api.Shared.WrapInTempList(
                    Server.World.GetGameObjectsOfProto<ILogicObject, TProtoEvent>());

            if (tempEvents.Count == 0)
            {
                return false;
            }

            var serverTime = Server.Game.FrameTime;
            foreach (var eventInstance in tempEvents.AsList())
            {
                if (eventInstance.GetPublicState<EventPublicState>().EventEndTime
                    > serverTime)
                {
                    return true;
                }
            }

            return false;
        }

        protected sealed override void ServerInitialize(ServerInitializeData data)
        {
            var activeEvent = data.GameObject;
            var publicState = data.PublicState;

            if (data.IsFirstTimeInit)
            {
                var serverTime = Server.Game.FrameTime;
                publicState.EventEndTime = serverTime + this.EventDuration.TotalSeconds;
                this.ServerLastActiveTime = serverTime;
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

                    WorldEventsSystem.ServerRegisterEvent(activeEvent);
                });

            this.ServerInitializeEvent(data);
        }

        protected abstract void ServerInitializeEvent(ServerInitializeData data);

        protected bool ServerIsSameEventExist()
        {
            using var tempEvents =
                Api.Shared.WrapInTempList(
                    Server.World.GetGameObjectsOfProto<ILogicObject, IProtoEvent>(this));

            return tempEvents.Count > 0;
        }

        protected abstract void ServerOnEventDestroyed(ILogicObject worldEvent);

        protected abstract void ServerOnEventStarted(ILogicObject worldEvent);

        protected virtual void ServerOnEventStartRequested(BaseTriggerConfig triggerConfig)
        {
            this.ServerCreateAndStartEventInstance();
        }

        protected abstract void ServerPrepareEvent(Triggers triggers);

        protected IServerZone ServerSelectRandomZoneWithEvenDistribution(
            IReadOnlyList<(IServerZone Zone, uint Weight)> list)
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

        protected virtual void ServerTryFinishEvent(ILogicObject worldEvent)
        {
            // destroy after a second delay
            // to ensure the public state is synchronized with the clients
            ServerTimersSystem.AddAction(
                1,
                () =>
                {
                    if (!worldEvent.IsDestroyed)
                    {
                        Server.World.DestroyObject(worldEvent);
                    }
                });
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var publicState = data.PublicState;
            var serverTime = Server.Game.FrameTime;

            var timeRemains = publicState.EventEndTime - serverTime;
            if (timeRemains > 0)
            {
                this.ServerLastActiveTime = serverTime;
                return;
            }

            this.ServerTryFinishEvent(data.GameObject);
        }

        protected virtual void ServerWorldChangedHandler()
        {
        }

        protected virtual void SharedPrepareEvent()
        {
        }

        private void ServerEventTriggerCallback(BaseTriggerConfig triggerConfig)
        {
            if (Api.IsEditor)
            {
                // event triggers are suppressed in Editor
                return;
            }

            var trigger = triggerConfig.Trigger;
            if (trigger is TriggerTimeInterval
                && Server.Game.HoursSinceWorldCreation < this.DelayHoursSinceWipe)
            {
                // too early
                PostponeEvent();
                return;
            }

            if (!this.ServerIsTriggerAllowed(trigger))
            {
                // cannot trigger now
                if (trigger is TriggerTimeInterval)
                {
                    PostponeEvent();
                }

                return;
            }

            this.ServerOnEventStartRequested(triggerConfig);

            void PostponeEvent()
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

                var postponeDuration = postponeDurationFrom
                                       + RandomHelper.NextDouble() * (postponeDurationTo - postponeDurationFrom);

                TriggerTimeInterval.ApplyPostpone(triggerConfig, postponeDuration);
                Logger.Important($"Event start postponed on {TimeSpan.FromSeconds(postponeDuration)} - {this}");
            }
        }
    }
}