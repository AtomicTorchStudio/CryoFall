namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Drones;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class ProtoItemDrone
        <TObjectDrone,
         TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemWithDurability
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemDrone
        where TObjectDrone : IProtoDrone, new()
        where TPrivateState : ItemDronePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        private static readonly Lazy<TObjectDrone> LazyProtoDrone
            = new(Api.GetProtoEntity<TObjectDrone>);

        private readonly Lazy<double> lazyDurabilityToStructurePointsConversionCoefficient;

        protected ProtoItemDrone()
        {
            this.lazyDurabilityToStructurePointsConversionCoefficient = new Lazy<double>(
                () => this.ProtoDrone.StructurePointsMax / this.DurabilityMax);
        }

        public double DurabilityToStructurePointsConversionCoefficient
            => this.lazyDurabilityToStructurePointsConversionCoefficient.Value;

        public override double GroundIconScale => 1.6;

        public override bool IsRepairable => true;

        public IProtoDrone ProtoDrone => LazyProtoDrone.Value;

        /// <summary>
        /// This number is used during drone selection when player sending a drone to mine something.
        /// </summary>
        public abstract int SelectionOrder { get; }

        public override void ServerOnDestroy(IItem gameObject)
        {
            base.ServerOnDestroy(gameObject);

            var objectDrone = GetPrivateState(gameObject).WorldObjectDrone;
            if (objectDrone is not null
                && !objectDrone.IsDestroyed)
            {
                Server.World.DestroyObject(objectDrone);
            }
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.DroneUsage);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            if (!data.IsFirstTimeInit)
            {
                return;
            }

            var itemDrone = data.GameObject;
            var protoDrone = LazyProtoDrone.Value;
            var objectDrone = Server.World.CreateDynamicWorldObject(
                protoDrone,
                CharacterDespawnSystem.ServerGetServiceAreaPosition().ToVector2D());
            protoDrone.ServerSetupAssociatedItem(objectDrone, itemDrone);
            data.PrivateState.WorldObjectDrone = objectDrone;
        }
    }

    public abstract class ProtoItemDrone<TObjectDrone>
        : ProtoItemDrone
            <TObjectDrone,
                ItemDronePrivateState,
                EmptyPublicState,
                EmptyClientState>
        where TObjectDrone : IProtoDrone, new()
    {
    }
}