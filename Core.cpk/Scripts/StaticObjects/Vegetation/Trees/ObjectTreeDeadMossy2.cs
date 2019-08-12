namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using System;

    public class ObjectTreeDeadMossy2 : ProtoObjectTree
    {
        // dead wood, so less hp than normal
        public override float StructurePointsMax => base.StructurePointsMax / 2;

        // no regeneration
        public override double StructurePointsRegenerationDurationSeconds => 0;

        // no growth time
        protected override TimeSpan TimeToMature => TimeSpan.Zero;

        public override string Name => "Dead tree";

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // primary drop
            droplist.Add<ItemLogs>(count: 3);
        }
    }
}