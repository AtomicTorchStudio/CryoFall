namespace AtomicTorch.CBND.CoreMod.Events.Base
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoEventWithArea
        <TEventPrivateState,
         TEventPublicState,
         TEventClientState>
        : ProtoEvent<
              TEventPrivateState,
              TEventPublicState,
              TEventClientState>,
          IProtoEventWithArea
        where TEventPrivateState : BasePrivateState, new()
        where TEventPublicState : EventWithAreaPublicState, new()
        where TEventClientState : BaseClientState, new()
    {
        protected static bool ServerCheckNoEventsInZone(IServerZone zoneInstance, List<ILogicObject> events)
        {
            foreach (var activeEvent in events)
            {
                var publicState = activeEvent.GetPublicState<EventWithAreaPublicState>();
                if (zoneInstance.IsContainsPosition(publicState.AreaEventOriginalPosition))
                {
                    // this event is in the same biome
                    return false;
                }
            }

            return true;
        }

        protected bool ServerCheckNoEventsNearby(
            Vector2Ushort position,
            double areaRadius)
        {
            using var tempAllActiveEvents =
                Api.Shared.WrapInTempList(
                    Server.World.GetGameObjectsOfProto<ILogicObject, IProtoEventWithArea>());

            return this.ServerCheckNoEventsNearby(position, areaRadius, tempAllActiveEvents.AsList());
        }

        protected bool ServerCheckNoEventsNearby(
            Vector2Ushort position,
            double areaRadius,
            List<ILogicObject> allActiveEvents)
        {
            var position2D = position.ToVector2D();
            foreach (var activeEvent in allActiveEvents)
            {
                var publicState = activeEvent.GetPublicState<EventWithAreaPublicState>();
                var distance = (publicState.AreaCirclePosition.ToVector2D() - position2D).Length;
                distance -= publicState.AreaCircleRadius;
                distance -= areaRadius;
                if (distance <= 0)
                {
                    // this event is too close
                    return false;
                }
            }

            return true;
        }

        protected bool ServerCheckNoEventsOfTypeInZone<TProtoEvent>(IServerZone zoneInstance)
            where TProtoEvent : class, IProtoEventWithArea
        {
            using var tempEvents =
                Api.Shared.WrapInTempList(
                    Server.World.GetGameObjectsOfProto<ILogicObject, TProtoEvent>());

            return ServerCheckNoEventsInZone(zoneInstance, tempEvents.AsList());
        }

        protected bool ServerCheckNoSameEventsInZone(IServerZone zoneInstance)
        {
            using var tempEvents =
                Api.Shared.WrapInTempList(
                    Server.World.GetGameObjectsOfProto<ILogicObject, IProtoEvent>(this));

            return ServerCheckNoEventsInZone(zoneInstance, tempEvents.AsList());
        }

        protected virtual void ServerCreateEventArea(
            ILogicObject activeEvent,
            ushort circleRadius,
            out Vector2Ushort circlePosition,
            out Vector2Ushort eventPosition)
        {
            var stopwatch = Stopwatch.StartNew();
            var world = Server.World;

            const int attemptsMax = 20;
            var attemptsRemains = attemptsMax;
            do
            {
                eventPosition = this.ServerPickEventPosition(activeEvent);

                // a valid spawn position found, try to create a search area circle which is including this location
                if (!this.ServerCreateEventSearchArea(world, eventPosition, circleRadius, out circlePosition))
                {
                    continue;
                }

                // search area created
                Logger.Important(
                    $"Generating an event area took {attemptsMax - attemptsRemains} attempts, {stopwatch.ElapsedMilliseconds}ms (for {this})");
                return;
            }
            while (--attemptsRemains > 0);

            throw new Exception("Unable to create the event area for " + activeEvent);
        }

        protected virtual bool ServerCreateEventSearchArea(
            IWorldServerService world,
            Vector2Ushort eventPosition,
            ushort circleRadius,
            out Vector2Ushort circlePosition)
        {
            var biome = world.GetTile(eventPosition).ProtoTile;
            return ServerSearchAreaHelper.GenerateSearchArea(eventPosition,
                                                             biome,
                                                             circleRadius,
                                                             out circlePosition,
                                                             maxAttempts: 100,
                                                             waterMaxRatio: 0.1);
        }

        protected bool ServerHasAnyEventOfType<TProtoEvent>()
            where TProtoEvent : class, IProtoEvent
        {
            using var tempEvents =
                Api.Shared.WrapInTempList(
                    Server.World.GetGameObjectsOfProto<ILogicObject, TProtoEvent>());

            return tempEvents.Count > 0;
        }

        protected bool ServerIsSameEventExist()
        {
            using var tempEvents =
                Api.Shared.WrapInTempList(
                    Server.World.GetGameObjectsOfProto<ILogicObject, IProtoEvent>(this));

            return tempEvents.Count > 0;
        }

        protected abstract bool ServerIsValidEventPosition(Vector2Ushort tilePosition);

        protected override void ServerOnEventStarted(ILogicObject activeEvent)
        {
            var publicState = GetPublicState(activeEvent);

            this.ServerCreateEventArea(activeEvent,
                                       this.AreaRadius,
                                       out var circlePosition,
                                       out var originalEventPosition);

            publicState.AreaEventOriginalPosition = originalEventPosition;
            publicState.AreaCirclePosition = circlePosition;
            publicState.AreaCircleRadius = this.AreaRadius;

            this.ServerOnEventWithAreaStarted(activeEvent);
        }

        protected abstract void ServerOnEventWithAreaStarted(ILogicObject activeEvent);

        protected abstract Vector2Ushort ServerPickEventPosition(ILogicObject activeEvent);
    }
}