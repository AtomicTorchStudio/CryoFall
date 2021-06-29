namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectTreeRubber : ProtoObjectTree
    {
        public override string Name => "Rubber tree";

        public override double TreeHeight => 2;

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(2);

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 3,
                rows: 1);
        }

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // primary drop
            droplist
                .Add<ItemLogs>(count: 5)
                .Add<ItemTwigs>(count: 2, countRandom: 2);

            // special drop
            droplist
                .Add<ItemRubberRaw>(count: 3, countRandom: 2);

            // saplings drop (requires skill)
            droplist
                .Add<ItemSaplingRubbertree>(condition: SkillLumbering.ConditionGetSapplings,
                                            count: 1,
                                            probability: 0.15)
                .Add<ItemSaplingRubbertree>(condition: SkillLumbering.ConditionGetExtraSapplings,
                                            count: 1,
                                            probability: 0.15);
        }

        protected override double ServerCalculateGrowthStageDuration(
            byte growthStage,
            VegetationPrivateState privateState,
            VegetationPublicState publicState)
        {
            var objectTree = (IStaticWorldObject)privateState.GameObject;
            var duration = base.ServerCalculateGrowthStageDuration(growthStage, privateState, publicState);
            if (LandClaimSystem.SharedIsObjectInsideAnyArea(objectTree))
            {
                return duration;
            }

            // x1.5 faster growth when not inside any land claim area as this resource
            // is rare and essential especially for PvP 
            return duration / 1.5;
        }
    }
}