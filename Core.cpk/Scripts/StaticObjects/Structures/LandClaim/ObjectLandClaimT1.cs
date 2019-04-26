namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectLandClaimT1 : ProtoObjectLandClaim
    {
        public override string Description =>
            "Helps you make your base safer by keeping other survivors from building or deconstructing anything in the surrounding area. Land claim also ensures that your structures do not decay over time.";

        public override TimeSpan DestructionTimeout => TimeSpan.FromHours(24);

        public override ushort LandClaimSize => 18;

        public override string Name => "Land claim (Tier 1)";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override byte SafeItemsSlotsCount => 12;

        public override float StructurePointsMax => 14000;

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var path = "StaticObjects/Structures/LandClaim/ObjectLandClaimT1";
            this.TextureResourceObjectBroken = new TextureResource(path + "Broken");
            return new TextureResource(path);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier3);
        }

        protected override void PrepareLandClaimConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryOther>();

            // important, due to issues with land claim area placement we had to make this 1 stage building
            build.StagesCount = 1;
            build.StageDurationSeconds = BuildDuration.Short; // irrelevant, since it is 1 stage building
            build.AddStageRequiredItem<ItemPlanks>(count: 100); // price reflects the entire cost
            build.AddStageRequiredItem<ItemStone>(count: 50);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 4);
            repair.AddStageRequiredItem<ItemStone>(count: 2);

            upgrade.AddUpgrade<ObjectLandClaimT2>()
                   .AddRequiredItem<ItemIngotIron>(count: 25)
                   .AddRequiredItem<ItemIngotCopper>(count: 25);
        }
    }
}