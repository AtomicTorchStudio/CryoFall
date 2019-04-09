namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectLandClaimT4 : ProtoObjectLandClaim
    {
        public override string Description => GetProtoEntity<ObjectLandClaimT1>().Description;

        public override string DescriptionUpgrade => GetProtoEntity<ObjectLandClaimT2>().DescriptionUpgrade;

        public override TimeSpan DestructionTimeout => TimeSpan.FromHours(36);

        public override ushort LandClaimSize => 24;

        public override string Name => "Land claim (Tier 4)";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override byte SafeItemsSlotsCount => 24;

        public override float StructurePointsMax => 42000;

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
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 5);
            repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);
        }
    }
}