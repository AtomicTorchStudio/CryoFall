namespace AtomicTorch.CBND.CoreMod.Events
{
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class SharedEventHelper
    {
        public static bool SharedIsInsideEventArea<TProtoEvent>(Vector2D worldPosition)
            where TProtoEvent : IProtoEventWithArea
        {
            var world = Api.IsServer
                            ? (IWorldService)Api.Server.World
                            : (IWorldService)Api.Client.World;

            using var tempAllActiveEvents =
                Api.Shared.WrapInTempList(
                    world.GetGameObjectsOfProto<ILogicObject, IProtoEventWithArea>());

            foreach (var activeEvent in tempAllActiveEvents.AsList())
            {
                if (activeEvent.ProtoGameObject is not TProtoEvent)
                {
                    continue;
                }

                var publicState = activeEvent.GetPublicState<EventWithAreaPublicState>();
                var distance = (publicState.AreaCirclePosition.ToVector2D() - worldPosition).Length;
                distance -= publicState.AreaCircleRadius;
                if (distance <= 0)
                {
                    // inside the area circle
                    return true;
                }
            }

            return false;
        }
    }
}