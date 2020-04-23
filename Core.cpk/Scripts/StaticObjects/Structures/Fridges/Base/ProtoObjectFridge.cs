namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
    using System;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class ProtoObjectFridge
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectCrate
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectFridge
        where TPrivateState : ObjectCratePrivateState, new()
        where TPublicState : ObjectCratePublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Lazy<IProtoItemsContainer> LazyFridgeContainer
            = new Lazy<IProtoItemsContainer>(Api.GetProtoEntity<ItemsContainerFridge>);

        public abstract double FreshnessDurationMultiplier { get; }

        public override bool IsSupportItemIcon => false;

        protected sealed override IProtoItemsContainer ItemsContainerType
            => LazyFridgeContainer.Value;

        public virtual double ServerGetCurrentFreshnessDurationMultiplier(IStaticWorldObject worldObject)
        {
            return this.FreshnessDurationMultiplier;
        }

        protected override BaseUserControlWithWindow ClientOpenUI(
            IStaticWorldObject worldObject,
            TPrivateState privateState)
        {
            return WindowFridge.Show(worldObject, privateState);
        }
    }

    public abstract class ProtoObjectFridge
        : ProtoObjectFridge<
            ObjectCratePrivateState,
            ObjectCratePublicState,
            StaticObjectClientState>
    {
    }
}