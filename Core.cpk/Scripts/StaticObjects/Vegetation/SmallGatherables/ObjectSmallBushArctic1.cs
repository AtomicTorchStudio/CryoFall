namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ObjectSmallBushArctic1 : ObjectSmallThornyShrub
    {
        public override string Name => "Small shrub";

        protected override bool CanFlipSprite => true;

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist
                .Add<ItemFibers>(count: 3, countRandom: 3)
                .Add<ItemTwigs>(count: 1,  countRandom: 1);

            // skill bonus
            droplist
                .Add<ItemFibers>(count: 2, probability: 1 / 5.0, condition: SkillForaging.ConditionAdditionalYield)
                .Add<ItemTwigs>(count: 1,  probability: 1 / 5.0, condition: SkillForaging.ConditionAdditionalYield);
        }
    }
}