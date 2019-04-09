namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectLandClaimT3 : ProtoObjectLandClaim
    {
        public override string Description => GetProtoEntity<ObjectLandClaimT1>().Description;

        public override string DescriptionUpgrade => GetProtoEntity<ObjectLandClaimT2>().DescriptionUpgrade;

        public override TimeSpan DestructionTimeout => TimeSpan.FromHours(32);

        public override ushort LandClaimSize => 22;

        public override string Name => "Land claim (Tier 3)";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override byte SafeItemsSlotsCount => 20;

        public override float StructurePointsMax => 36000;

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var path = "StaticObjects/Structures/LandClaim/ObjectLandClaimT3";
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
            repair.AddStageRequiredItem<ItemIngotIron>(count: 3);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 3);
            repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            upgrade.AddUpgrade<ObjectLandClaimT4>()
                   .AddRequiredItem<ItemPlastic>(count: 50)
                   .AddRequiredItem<ItemIngotCopper>(count: 100)
                   .AddRequiredItem<ItemComponentsHighTech>(count: 20)
                   .AddRequiredItem<ItemPowerCell>(count: 5);
        }
    }
}