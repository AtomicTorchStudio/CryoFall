namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ObjectLandClaimT1 : ProtoObjectLandClaim
    {
        public override TimeSpan DecayDelayDuration { get; } = TimeSpan.FromHours(56);

        public override string Description =>
            "Helps you make your base safer by keeping other survivors from building or deconstructing anything in the surrounding area. Land claim also ensures that your structures do not decay over time.";

        public override TimeSpan DestructionTimeout { get; } = TimeSpan.FromHours(1);

        public override double HitRaidblockDurationMultiplier => 0.1;

        public override ushort LandClaimSize => 14;

        public override byte LandClaimTier => 1;

        public override string Name => "Land claim (Tier 1)";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        // no shield protection in T1 as there are is no electricity until T2
        public override double ShieldProtectionDuration => 0;

        public override double ShieldProtectionTotalElectricityCost => 0;

        public override float StructurePointsMax => 2000;

        protected override BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject)
        {
            return null;
        }

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
            build.StageDurationSeconds = BuildDuration.Short;   // irrelevant, since it is 1 stage building
            build.AddStageRequiredItem<ItemPlanks>(count: 100); // price reflects the entire cost
            build.AddStageRequiredItem<ItemStone>(count: 50);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 5);

            upgrade.AddUpgrade<ObjectLandClaimT2>()
                   .AddRequiredItem<ItemIngotIron>(count: 10)
                   .AddRequiredItem<ItemIngotCopper>(count: 10)
                   .AddRequiredItem<ItemPlanks>(count: 25);
        }
    }
}