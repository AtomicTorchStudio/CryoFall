namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectOilPump : ProtoObjectExtractor
    {
        public const string ErrorRequiresOilSeep = "The pump requires an oil seep.";

        private static readonly ConstructionTileRequirements.Validator ValidatorForPvP
            = new ConstructionTileRequirements.Validator(
                ErrorRequiresOilSeep,
                c =>
                {
                    if (PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                    {
                        // don't validate this condition on PvE servers
                        return true;
                    }

                    foreach (var obj in c.Tile.StaticObjects)
                    {
                        if (obj.ProtoStaticWorldObject is ObjectDepositOilSeep)
                        {
                            return true;
                        }
                    }

                    return false;
                });

        private static readonly ConstructionTileRequirements.Validator ValidatorForPvE
            = new ConstructionTileRequirements.Validator(
                () => "[b]"
                      + ConstructionTileRequirements.Error_UnsuitableGround_Title
                      + "[/b]"
                      + "[br]"
                      + ConstructionTileRequirements.Error_UnsuitableGround_Message_CanBuildOnlyOn
                      + "[*]"
                      + Api.GetProtoEntity<TileBarren>().Name,
                c =>
                {
                    if (!PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                    {
                        // don't validate this condition on PvP servers
                        return true;
                    }

                    return c.Tile.ProtoTile == Api.GetProtoEntity<TileBarren>();
                });

        private readonly TextureAtlasResource textureAtlasOilPumpActive;

        public ObjectOilPump()
        {
            this.textureAtlasOilPumpActive = new TextureAtlasResource(
                this.GenerateTexturePath() + "Active",
                columns: 8,
                rows: 2,
                isTransparent: true);
        }

        public override byte ContainerFuelSlotsCount => 1;

        public override byte ContainerInputSlotsCount => 1;

        public override byte ContainerOutputSlotsCount => 1;

        public override string Description =>
            "Can be built over an oil seep to extract raw petroleum oil directly from the ground.";

        public override float LiquidCapacity => 100;

        public override float LiquidProductionAmountPerSecond => 0.1f;

        public override string Name => "Oil pump";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 10000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var animatedSpritePositionOffset = (300 / (double)ScriptingConstants.TileSizeRealPixels,
                                                376 / (double)ScriptingConstants.TileSizeRealPixels);

            this.ClientSetupOilPumpActiveAnimation(
                data.GameObject,
                data.SyncPublicState,
                this.textureAtlasOilPumpActive,
                animatedSpritePositionOffset,
                frameDurationSeconds: 0.04,
                autoInverseAnimation: true);
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            var objectDeposit = data.GameObject.OccupiedTile.StaticObjects.FirstOrDefault(
                o => o.ProtoStaticWorldObject is ObjectDepositOilSeep);

            var privateState = data.SyncPrivateState;
            return WindowOilPump.Open(
                new ViewModelWindowOilPump(
                    data.GameObject,
                    objectDeposit,
                    privateState.ManufacturingState,
                    this.ManufacturingConfig,
                    privateState.FuelBurningState,
                    privateState.LiquidContainerState,
                    this.LiquidContainerConfig));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 1.8;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###",
                         "###");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryIndustry>();

            // Oil pump requires each tile to contain an oil seep.
            // Notice: technically it will be possible to construct one oil pump on two oil seeps if they're nearby,
            // so there should be the oil spawn limitation to avoid that case!
            tileRequirements
                .Clear()
                .Add(ValidatorForPvP)
                .Add(ValidatorForPvE)
                .Add(ConstructionTileRequirements.BasicRequirements)
                .Add(ConstructionTileRequirements.ValidatorClientOnlyNoCurrentPlayer)
                .Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyDynamic)
                .Add(ConstructionTileRequirements.ErrorNoFreeSpace,
                     c => !ConstructionTileRequirements.TileHasAnyPhysicsObjectsWhere(
                              c.Tile,
                              o => o.PhysicsBody.IsStatic
                                   && !(o.PhysicsBody.AssociatedWorldObject?.ProtoWorldObject
                                            is ObjectDepositOilSeep)))
                .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                .Add(LandClaimSystem.ValidatorIsOwnedOrFreeArea);

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 20);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 15);
            build.AddStageRequiredItem<ItemCement>(count: 25);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 2);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            // use the oil pump active sound
            return base.PrepareSoundPresetObject().Clone().Replace(
                ObjectSound.Active,
                "Objects/Structures/" + nameof(ObjectOilPump) + "/Active");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((2.8, 2.2), (0.1, 0))
                .AddShapeRectangle((2.8, 2.4), (0.1, 0), CollisionGroups.HitboxMelee)
                .AddShapeRectangle((2.8, 2.6), (0.1, 0), CollisionGroups.HitboxRanged)
                .AddShapeRectangle((2.8, 2.8), (0.1, 0), CollisionGroups.ClickArea);
        }

        protected override IStaticWorldObject SharedGetDepositWorldObject(Tile tile)
        {
            return tile.StaticObjects.FirstOrDefault(
                o => o.ProtoStaticWorldObject is ObjectDepositOilSeep);
        }
    }
}