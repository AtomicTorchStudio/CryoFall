namespace AtomicTorch.CBND.CoreMod.Events.Base
{
    using System;
    using System.Diagnostics;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.ServicesServer;
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
        protected virtual void ServerCreateEventArea(
            ILogicObject activeEvent,
            ushort circleRadius,
            out Vector2Ushort circlePosition)
        {
            var stopwatch = Stopwatch.StartNew();
            var world = Server.World;

            const int attemptsMax = 1000;
            var attemptsRemains = attemptsMax;
            do
            {
                var eventPosition = this.ServerPickEventPosition(activeEvent);

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
                                                             maxAttempts: 100);
        }

        protected abstract bool ServerIsValidEventPosition(Vector2Ushort tilePosition);

        protected override void ServerOnEventStarted(ILogicObject activeEvent)
        {
            var publicState = GetPublicState(activeEvent);

            this.ServerCreateEventArea(activeEvent,
                                       this.AreaRadius,
                                       out var circlePosition);

            publicState.AreaCirclePosition = circlePosition;
            publicState.AreaCircleRadius = this.AreaRadius;

            this.ServerOnEventWithAreaStarted(activeEvent);
        }

        protected abstract void ServerOnEventWithAreaStarted(ILogicObject activeEvent);

        protected abstract Vector2Ushort ServerPickEventPosition(ILogicObject activeEvent);
    }
}