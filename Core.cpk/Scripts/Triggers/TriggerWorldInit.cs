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
            if (Api.Shared.IsDebug)
            {
                return;
            }

            this.Invoke();
        }
    }
}