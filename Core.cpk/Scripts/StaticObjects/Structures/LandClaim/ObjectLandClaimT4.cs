namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ObjectLandClaimT4 : ProtoObjectLandClaim
    {
        public override TimeSpan DecayDelayDuration { get; } = TimeSpan.FromHours(128);

        public override string Description => GetProtoEntity<ObjectLandClaimT1>().Description;

        public override string DescriptionUpgrade => GetProtoEntity<ObjectLandClaimT2>().DescriptionUpgrade;

        public override TimeSpan DestructionTimeout { get; } = TimeSpan.FromHours(36);

        public override ushort LandClaimSize => 22;

        public override string Name => "Land claim (Tier 4)";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 42000;

        protected override BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: LightColors.ElectricCold,
                size: (5, 9.5),
                positionOffset: (1, 1.45));
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var path = "StaticObjects/Structures/LandClaim/ObjectLandClaimT4";
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
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 4);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 4);

            upgrade.AddUpgrade<ObjectLandClaimT5>()
                   .AddRequiredItem<ItemPlastic>(count: 50)
                   .AddRequiredItem<ItemOrePragmium>(count: 50)
                   .AddRequiredItem<ItemComponentsHighTech>(count: 20)
                   .AddRequiredItem<ItemPowerCell>(count: 5);
        }
    }
}