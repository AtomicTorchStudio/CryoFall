namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using System;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class ProtoObjectCrate
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IInteractableProtoStaticWorldObject,
          IProtoObjectWithOwnersList,
          IProtoObjectWithAccessMode
        where TPrivateState : ObjectCratePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        // how long the items dropped on the ground from the destroyed crate should remain there
        private static readonly TimeSpan DestroyedCrateDroppedItemsDestructionTimeout = TimeSpan.FromDays(1);

        public override string InteractionTooltipText => InteractionTooltipTexts.Open;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public bool IsClosedAccessModeAvailable => false;

        public abstract byte ItemsSlotsCount { get; }

        public override float StructurePointsMaxForConstructionSite
            => this.StructurePointsMax / 25;

        protected virtual IProtoItemsContainer ItemsContainerType
            => Api.GetProtoEntity<ItemsContainerDefault>();

        public override void ServerOnBuilt(IStaticWorldObject structure, ICharacter byCharacter)
        {
            WorldObjectOwnersSystem.ServerOnBuilt(structure, byCharacter);
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            var itemsContainer = GetPrivateState(gameObject).ItemsContainer;
            if (itemsContainer.OccupiedSlotsCount == 0)
            {
                return;
            }

            var groundContainer = ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                itemsContainer);

            if (groundContainer != null)
            {
                // set custom timeout for the dropped ground items container
                ObjectGroundItemsContainer.ServerSetDestructionTimeout(
                    (IStaticWorldObject)groundContainer.Owner,
                    DestroyedCrateDroppedItemsDestructionTimeout.TotalSeconds);
            }
        }

        public bool SharedCanEditOwners(IStaticWorldObject worldObject, ICharacter byOwner)
        {
            return true;
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            return base.SharedCanInteract(character, worldObject, writeToLog)
                   && (CreativeModeSystem.SharedIsInCreativeMode(character)
                       || WorldObjectAccessModeSystem.SharedHasAccess(character, worldObject, writeToLog));
        }

        BaseUserControlWithWindow IInteractableProtoStaticWorldObject.ClientOpenUI(IStaticWorldObject worldObject)
        {
            var privateState = GetPrivateState(worldObject);
            return this.ClientOpenUI(worldObject, privateState);
        }

        void IInteractableProtoStaticWorldObject.ServerOnClientInteract(ICharacter who, IStaticWorldObject worldObject)
        {
        }

        void IInteractableProtoStaticWorldObject.ServerOnMenuClosed(ICharacter who, IStaticWorldObject worldObject)
        {
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableStaticWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected virtual BaseUserControlWithWindow ClientOpenUI(
            IStaticWorldObject worldObject,
            TPrivateState privateState)
        {
            return WindowCrateContainer.Show(worldObject, privateState);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var worldObject = data.GameObject;
            var privateState = data.PrivateState;
            if (data.IsFirstTimeInit)
            {
                privateState.AccessMode = WorldObjectAccessMode.OpensToObjectOwnersOrAreaOwners;
            }

            WorldObjectOwnersSystem.ServerInitialize(worldObject);

            var itemsContainer = privateState.ItemsContainer;
            if (itemsContainer != null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: this.ItemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer(
                owner: worldObject,
                itemsContainerType: this.ItemsContainerType,
                slotsCount: this.ItemsSlotsCount);

            privateState.ItemsContainer = itemsContainer;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 0.5), offset: (0, 0.4))
                .AddShapeRectangle(
                    size: (1, 0.75),
                    offset: (0, 0.4),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (1, 0.75),
                    offset: (0, 0.4),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(
                    size: (1, 0.75),
                    offset: (0, 0.4),
                    group: CollisionGroups.ClickArea);
        }
    }

    public abstract class ProtoObjectCrate
        : ProtoObjectCrate<ObjectCratePrivateState, StaticObjectPublicState,
            StaticObjectClientState>
    {
    }
}