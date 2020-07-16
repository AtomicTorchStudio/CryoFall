namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ObjectLandClaimT2 : ProtoObjectLandClaim
    {
        public override TimeSpan DecayDelayDuration { get; } = TimeSpan.FromHours(80);

        public override string Description => GetProtoEntity<ObjectLandClaimT1>().Description;

        public override string DescriptionUpgrade =>
            "Increases protected area, maximum structural integrity and destruction delay.";

        public override TimeSpan DestructionTimeout { get; } = TimeSpan.FromHours(28);

        public override ushort LandClaimSize => 16;

        public override byte LandClaimTier => 2;

        public override string Name => "Land claim (Tier 2)";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override double ShieldProtectionDuration => 52 * 60 * 60; // 52 hours

        public override double ShieldProtectionTotalElectricityCost => 5000;

        public override float StructurePointsMax => 25000;

        protected override BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: LightColors.ElectricCold,
                size: (4, 8),
                positionOffset: (0.95, 1.3));
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var path = "StaticObjects/Structures/LandClaim/ObjectLandClaimT2";
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

            // build is not allowed - it's an upgrade from previous level
            // for requirements for the upgrade see construction upgrade config from previous level
            build.IsAllowed = false;

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotIron>(count: 2);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 2);

            upgrade.AddUpgrade<ObjectLandClaimT3>()
                   .AddRequiredItem<ItemIngotSteel>(count: 25)
                   .AddRequiredItem<ItemIngotCopper>(count: 25);
        }
    }
}