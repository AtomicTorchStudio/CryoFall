namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLandClaimT5 : ProtoObjectLandClaim
    {
        public override TimeSpan DecayDelayDuration { get; } = TimeSpan.FromHours(176);

        public override string Description => GetProtoEntity<ObjectLandClaimT1>().Description;

        public override string DescriptionUpgrade => GetProtoEntity<ObjectLandClaimT2>().DescriptionUpgrade;

        public override TimeSpan DestructionTimeout { get; } = TimeSpan.FromHours(48);

        public override ushort LandClaimSize => 22;

        public override byte LandClaimTier => 5;

        public override string Name => "Land claim (Tier 5)";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override double ShieldProtectionDuration => 52 * 60 * 60; // 52 hours

        public override double ShieldProtectionTotalElectricityCost => 5000;

        public override float StructurePointsMax => 50000;

        protected override BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: LightColors.ElectricCold,
                size: (6, 11),
                positionOffset: (1, 1.9));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.55;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var path = "StaticObjects/Structures/LandClaim/ObjectLandClaimT5";
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
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 5);
        }
    }
}