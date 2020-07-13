namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodeTrashCan : TechNode<TechGroupConstructionT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectTrashCan>();

            config.SetRequiredNode<TechNodeBed>();
        }
    }
}