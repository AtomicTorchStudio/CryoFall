namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class ObjectGateSteel : ProtoObjectGate
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
    }
}