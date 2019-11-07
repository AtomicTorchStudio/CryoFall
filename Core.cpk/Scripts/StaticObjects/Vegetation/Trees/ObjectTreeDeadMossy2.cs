namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ObjectTreeDeadMossy2 : ProtoObjectTree
    {
        public override string Name => "Dead tree";

        // dead wood, so less hp than normal
        public override float StructurePointsMax => base.StructurePointsMax / 2;

        // no regeneration
        public override double StructurePointsRegenerationDurationSeconds => 0;

        // no growth time
        protected override TimeSpan TimeToMature => TimeSpan.Zero;

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // primary drop
            droplist.Add<ItemLogs>(count: 3);
        }
    }
}