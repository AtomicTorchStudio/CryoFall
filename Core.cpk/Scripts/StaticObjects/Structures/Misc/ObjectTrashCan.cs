namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
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
          IInteractableProtoWorldObject
    {
        public override string Description =>
            "You can use this trash can to immediately dispose of any unwanted items. Permanently!";

        public override string InteractionTooltipText => InteractionTooltipTexts.Open;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override bool IsRelocatable => true;

        public override string Name => "Trash can";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 500;

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            if (LandClaimSystem.SharedIsObjectInsideOwnedOrFreeArea(worldObject, character)
                || CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            // not the land owner
            if (writeToLog)
            {
                Logger.Warning(
                    $"Character cannot interact with {worldObject} - not the land owner.",
                    character);

                if (IsClient)
                {
                    WorldObjectOwnersSystem.ClientOnCannotInteractNotOwner(worldObject);
                }
            }

            return false;
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            var staticWorldObject = (IStaticWorldObject)worldObject;
            var privateState = GetPrivateState(staticWorldObject);
            return this.ClientOpenUI(staticWorldObject, privateState);
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
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
            tileRequirements.Add(LandClaimSystem.ValidatorIsOwnedLand);

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

            var itemsContainer = data.PrivateState.ItemsContainer;
            const byte itemsSlotsCount = 4;
            if (itemsContainer is not null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: itemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer<ItemsContainerTrashCan>(
                owner: data.GameObject,
                slotsCount: itemsSlotsCount);

            data.PrivateState.ItemsContainer = itemsContainer;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.7, 0.35), offset: (0.15, 0.4))
                .AddShapeRectangle(size: (0.6, 0.65), offset: (0.2, 0.4),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.7, 0.2),  offset: (0.15, 1.1), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.6, 0.8),  offset: (0.2, 0.4),  group: CollisionGroups.ClickArea);
        }

        public class PrivateState : StructurePrivateState
        {
            [SyncToClient]
            public IItemsContainer ItemsContainer { get; set; }
        }
    }
}