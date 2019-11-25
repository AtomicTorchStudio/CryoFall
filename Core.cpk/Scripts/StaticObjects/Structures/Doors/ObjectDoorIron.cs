namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectDoorIron : ProtoObjectDoor
    {
        public ObjectDoorIron()
        {
            var texturePath = this.GenerateTexturePath();

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
            "Good defense against most intruders. Still, it won't stand much of a chance against explosives.";

        public override string Name => "Iron door";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override double StructureExplosiveDefenseCoef => 0.25;

        public override float StructurePointsMax => 30000;

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryBuildings>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotIron>(count: 2);
            build.AddStageRequiredItem<ItemCement>(count: 5);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotIron>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier2);
        }
    }
}