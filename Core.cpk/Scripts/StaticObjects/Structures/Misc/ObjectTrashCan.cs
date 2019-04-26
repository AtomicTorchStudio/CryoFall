namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectTrashCan
        : ProtoObjectStructure
          <ObjectTrashCan.PrivateState,
              StaticObjectPublicState,
              StaticObjectClientState>,
          IInteractableProtoStaticWorldObject
    {
        public override string Description =>
            "You can use this trash can to immediately dispose of any unwanted items. Permanently!";

        public override string InteractionTooltipText => InteractionTooltipTexts.Open;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override string Name => "Trash can";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 250;

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
            PrivateState privateState)
        {
            return WindowTrashCan.Show(privateState);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0, 0.4);
            renderer.DrawOrderOffsetY = 0.1;
            renderer.Scale = 0.8;
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryOther>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotCopper>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Default);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var worldObject = data.GameObject;

            var itemsContainer = data.PrivateState.ItemsContainer;
            const byte itemsSlotsCount = 4;
            if (itemsContainer != null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: itemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer<ItemsContainerTrashCan>(
                owner: worldObject,
                slotsCount: itemsSlotsCount);

            data.PrivateState.ItemsContainer = itemsContainer;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.7, 0.35),
                                   offset: (0.15, 0.4),
                                   group: CollisionGroups.Default)
                .AddShapeRectangle(size: (0.6, 0.65),
                                   offset: (0.2, 0.4),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.6, 0.65),
                                   offset: (0.2, 0.4),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.6, 0.8),
                                   offset: (0.2, 0.4),
                                   group: CollisionGroups.ClickArea);
        }

        public class PrivateState : StructurePrivateState
        {
            [SyncToClient]
            public IItemsContainer ItemsContainer { get; set; }
        }
    }
}