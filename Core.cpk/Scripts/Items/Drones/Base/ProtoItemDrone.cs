namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Drones;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
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
        where TPrivateState : ItemDronePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
        where TObjectDrone : IProtoDrone, new()
    {
        private static readonly Lazy<TObjectDrone> LazyProtoDrone
            = new Lazy<TObjectDrone>(Api.GetProtoEntity<TObjectDrone>);

        private readonly Lazy<double> lazyDurabilityToStructurePointsConversionCoefficient;

        protected ProtoItemDrone()
        {
            this.Icon = new TextureResource("Items/Drones/" + this.GetType().Name);
            this.lazyDurabilityToStructurePointsConversionCoefficient = new Lazy<double>(
                () => this.ProtoDrone.StructurePointsMax / this.DurabilityMax);
        }

        public double DurabilityToStructurePointsConversionCoefficient
            => this.lazyDurabilityToStructurePointsConversionCoefficient.Value;

        public override double GroundIconScale => 1.6;

        public override ITextureResource Icon { get; }

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
            if (objectDrone != null
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

            var item = data.GameObject;
            var protoDrone = LazyProtoDrone.Value;
            var objectDrone = Server.World.CreateDynamicWorldObject(
                protoDrone,
                ServerCharacterDeathMechanic.ServerGetGraveyardPosition().ToVector2D());
            protoDrone.ServerSetupAssociatedItem(objectDrone, item);
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