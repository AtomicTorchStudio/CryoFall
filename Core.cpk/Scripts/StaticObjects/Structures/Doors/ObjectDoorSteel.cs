namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectDoorSteel : ProtoObjectDoor
    {
        public ObjectDoorSteel()
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
            "Steel certainly deserves its reputation. It will hardly buckle against anything but the most powerful explosives.";

        public override string Name => "Steel door";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override double StructureExplosiveDefenseCoef => 0.5;

        public override float StructurePointsMax => 40000;

        protected override void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryBuildings>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 4);
            build.AddStageRequiredItem<ItemComponentsMechanical>(count: 1);
            build.AddStageRequiredItem<ItemCement>(count: 15);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 2);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier3);
        }
    }
}