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

        public abstract string Description { get; }

        public abstract TimeSpan EventDuration { get; }

        public virtual ITextureResource Icon { get; }

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override string ShortId { get; }

        public IReadOnlyList<BaseTriggerConfig> Triggers { get; private set; }

        public void ServerForceCreateAndStart()
        {
            var activeEvent = Server.World.CreateLogicObject(this);
            Logger.Important("Event created: " + activeEvent);

            try
            {
                this.ServerOnEventStarted(activeEvent);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Error when starting an event. The event will be destroyed: " + activeEvent);
                Server.World.DestroyObject(activeEvent);
            }
        }

        public sealed override void ServerOnDestroy(ILogicObject gameObject)
        {
            ActiveEventsSystem.ServerUnregisterEvent(gameObject);
            this.ServerOnEventDestroyed(gameObject);
            Logger.Important("Event destroyed: " + gameObject);
        }

        public abstract string SharedGetProgressText(ILogicObject activeEvent);

        protected static TProtoTrigger GetTrigger<TProtoTrigger>()
            where TProtoTrigger : ProtoTrigger, new()
        {
            return Api.GetProtoEntity<TProtoTrigger>();
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
                    callback: () => this.TriggerCallback(triggerConfig),
                    $"Event.{this.GetType().Name}");
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

        public virtual bool ServerIsTriggerAllowed(ProtoTrigger trigger) => true;

        protected abstract void ServerOnEventDestroyed(ILogicObject activeEvent);

        protected abstract void ServerOnEventStarted(ILogicObject activeEvent);

        protected abstract void ServerPrepareEvent(Triggers triggers);

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

        private void TriggerCallback(BaseTriggerConfig triggerConfig)
        {
            if (Api.IsEditor)
            {
                // event triggers are suppressed in Editor
                return;
            }

            if (!this.ServerIsTriggerAllowed(triggerConfig.Trigger))
            {
                return;
            }

            this.ServerForceCreateAndStart();
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
    }
}