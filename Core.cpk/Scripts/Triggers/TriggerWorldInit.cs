namespace AtomicTorch.CBND.CoreMod.Triggers
{
    using AtomicTorch.CBND.GameApi;

    public class TriggerWorldInit : ProtoTriggerNonConfigurable
    {
        [NotLocalizable]
        public override string Name => "World init trigger";

        public void OnWorldInit()
        {
            this.Invoke();
        }
    }
}