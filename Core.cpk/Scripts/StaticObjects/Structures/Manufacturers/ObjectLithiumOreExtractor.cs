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

    public class ObjectLithiumOreExtractor : ProtoObjectExtractor
    {
        public const string ErrorRequiresGeothermalSpring = "The extractor requires a geothermal spring.";

        private static readonly ConstructionTileRequirements.Validator ValidatorForPvP
            = new ConstructionTileRequirements.Validator(
                ErrorRequiresGeothermalSpring,
                c =>
                {
                    if (PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                    {
                        // don't validate this condition on PvE servers
                        return true;
                    }

                    foreach (var obj in c.Tile.StaticObjects)
                    {
                        if (obj.ProtoStaticWorldObject is ObjectDepositGeothermalSpring)
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
                      + Api.GetProtoEntity<TileForestTemperate>().Name
                      + "[*]"
                      + Api.GetProtoEntity<TileForestBoreal>().Name,
                c =>
                {
                    if (!PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                    {
                        // don't validate this condition on PvP servers
                        return true;
                    }

                    return c.Tile.ProtoTile is TileForestTemperate
                           || c.Tile.ProtoTile is TileForestBoreal;
                });

        private readonly TextureAtlasResource textureAtlasActive1;

        private readonly TextureAtlasResource textureAtlasActive2;

        public ObjectLithiumOreExtractor()
        {
            var texturePath = this.GenerateTexturePath();
            this.textureAtlasActive1 = new TextureAtlasResource(
                texturePath + "Active1",
                columns: 8,
                rows: 1,
                isTransparent: true);

            this.textureAtlasActive2 = new TextureAtlasResource(
                texturePath + "Active2",
                columns: 8,
                rows: 1,
                isTransparent: true);
        }

        public override byte ContainerFuelSlotsCount => 1;

        public override byte ContainerInputSlotsCount => 0;

        public override byte ContainerOutputSlotsCount => 1;

        public override string Description => "Allows extraction of lithium salts from geothermal springs.";

        public override float LiquidCapacity => 100;

        public override float LiquidProductionAmountPerSecond => 1;

        public override string Name => "Lithium salts extractor";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 10000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            this.ClientSetupOilPumpActiveAnimation(
                data.GameObject,
                data.SyncPublicState,
                this.textureAtlasActive1,
                positionOffset: (
                                    640 / (double)ScriptingConstants.TileSizeRealPixels,
                                    351 / (double)ScriptingConstants.TileSizeRealPixels),
                frameDurationSeconds: 0.04,
                autoInverseAnimation: true);

            this.ClientSetupOilPumpActiveAnimation(
                data.GameObject,
                data.SyncPublicState,
                this.textureAtlasActive2,
                positionOffset: (
                                    134 / (double)ScriptingConstants.TileSizeRealPixels,
                                    417 / (double)ScriptingConstants.TileSizeRealPixels),
                frameDurationSeconds: 0.03,
                autoInverseAnimation: false,
                playSounds: false);
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            var objectDeposit = data.GameObject.OccupiedTile.StaticObjects.FirstOrDefault(
                o => o.ProtoStaticWorldObject is ObjectDepositGeothermalSpring);

            var privateState = data.SyncPrivateState;
            return WindowLithiumOreExtractor.Open(
                new ViewModelWindowLithiumOreExtractor(
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

            // Lithium salt extractor requires each tile to contain a geothermal spring.
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
                              t => t.PhysicsBody.IsStatic
                                   && !(t.PhysicsBody.AssociatedWorldObject?.ProtoWorldObject is
                                            ObjectDepositGeothermalSpring)))
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
                "Objects/Structures/" + nameof(ObjectLithiumOreExtractor) + "/Active");
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
                o => o.ProtoStaticWorldObject is ObjectDepositGeothermalSpring);
        }
    }
}