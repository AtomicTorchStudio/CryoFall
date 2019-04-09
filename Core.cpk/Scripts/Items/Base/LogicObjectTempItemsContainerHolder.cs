namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    /// <summary>
    /// This prototype is useful when we need a dummy logic object to use as a temp items container owner.
    /// </summary>
    public class LogicObjectTempItemsContainerHolder
        : ProtoGameObject
          <ILogicObject,
              EmptyPrivateState,
              EmptyPublicState,
              EmptyClientState>,
          IProtoLogicObject
    {
        public override double ClientUpdateIntervalSeconds => 0;

        [NotLocalizable]
        public override string Name => "Temp items container owner";

        public override double ServerUpdateIntervalSeconds => 0;
    }
}