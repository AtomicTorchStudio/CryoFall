namespace AtomicTorch.CBND.CoreMod.Triggers
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TriggerWorldInit : ProtoTriggerNonConfigurable
    {
        [NotLocalizable]
        public override string Name => "World init trigger";

        public void OnWorldInit()
        {
            if (!Server.Database.TryGet("Core", "IsWorldSpawned", out bool isWorldSpawned)
                || !isWorldSpawned)
            {
                Server.Database.Set("Core", "IsWorldSpawned", true);
            }

            if (isWorldSpawned
                && Api.Shared.IsDebug)
            {
                // In the debug mode, don't invoke world init trigger as the world
                // is already spawned with objects and we don't want to have
                // any additional delay (ensure debug client connections ASAP). 
                return;
            }

            this.Invoke();
        }
    }
}