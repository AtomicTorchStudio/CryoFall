namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ObjectGateSteel : ProtoObjectDoor
    {
        protected ObjectGateSteel()
        {
            var folderPath = SharedGetRelativeFolderPath(this.GetType(), typeof(ProtoStaticWorldObject<,,>));
            var texturePath = $"StaticObjects/{folderPath}/{nameof(ObjectGateSteel)}";

            this.AtlasTextureHorizontal = new TextureAtlasResource(
                texturePath + "Horizontal",
                columns: 6,
                rows: 1,
                isTransparent: true);

            this.TextureBaseHorizontal = new TextureResource(
                texturePath + "HorizontalBase",
                isTransparent: true);

            this.AtlasTextureVertical = new TextureAtlasResource(
                texturePath + "Vertical",
                columns: 8,
                rows: 1,
                isTransparent: true);
        }

        public override string Description =>
            "Large steel gate that could be conveniently used as the main entrance to your base. Gates are also necessary for large vehicles.";

        public override int DoorSizeTiles => 2;

        public override bool IsHeavyVehicleCanPass => true;

        public override string Name => "Steel gate";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override double StructureExplosiveDefenseCoef => 0.5;

        public override float StructurePointsMax => 50000;

        protected override void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryBuildings>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 6);
            build.AddStageRequiredItem<ItemCement>(count: 5);
            build.AddStageRequiredItem<ItemComponentsMechanical>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 4);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier3);
        }

        // the gates are double size so we need to increase the check bounds
        protected override void PrepareOpeningBounds(out BoundsDouble horizontal, out BoundsDouble vertical)
        {
            var distance = 1.6;
            var distanceLarge = distance + 0.5;
            horizontal = new BoundsDouble(minX: -distanceLarge,
                                          minY: -distance,
                                          maxX: distanceLarge,
                                          maxY: distance);

            vertical = new BoundsDouble(minX: -distance,
                                        minY: -distanceLarge,
                                        maxX: distance,
                                        maxY: distanceLarge);
        }

        protected override BoundsDouble SharedGetDoorOpeningBounds(IStaticWorldObject worldObject)
        {
            var isHorizontalDoor = GetPublicState(worldObject).IsHorizontalDoor;
            var objectOpeningBounds = this.SharedGetDoorOpenBounds(isHorizontalDoor);
            var offset = isHorizontalDoor
                             ? new Vector2D(1.0, 0.1)
                             : new Vector2D(0.5, 1.0);

            var tilePosition = worldObject.TilePosition;
            var boundsOffset = objectOpeningBounds.Offset;
            return new BoundsDouble(
                new Vector2D(
                    boundsOffset.X + offset.X + tilePosition.X,
                    boundsOffset.Y + offset.Y + tilePosition.Y),
                objectOpeningBounds.Size);
        }
    }
}