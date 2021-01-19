namespace AtomicTorch.CBND.CoreMod.Events
{
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientWorldEventWatcher
    {
        private static void LogicObjectDeinitializedHandler(ILogicObject obj)
        {
            if (obj.ProtoGameObject is IProtoEvent)
            {
                ClientWorldEventRegularNotificationManager.UnregisterEvent(obj);
                ClientWorldEventConsolidatedNotificationManager.UnregisterEvent(obj);
            }
        }

        private static void LogicObjectInitializedHandler(ILogicObject obj)
        {
            if (obj.ProtoGameObject is IProtoEvent)
            {
                ClientWorldEventRegularNotificationManager.RegisterEvent(obj);
                ClientWorldEventConsolidatedNotificationManager.RegisterEvent(obj);
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Api.Client.World.LogicObjectInitialized += LogicObjectInitializedHandler;
                Api.Client.World.LogicObjectDeinitialized += LogicObjectDeinitializedHandler;
            }
        }
    }
}