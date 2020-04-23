namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;

    // The only purpose of this inherited construction site
    // is to provide increased scope size to prevent visual issues
    // when placing blueprints for other extractors.
    public class ObjectConstructionSiteForExtractor : ObjectConstructionSite
    {
        public override bool HasIncreasedScopeSize => true;

        [NotLocalizable]
        public override string Name => base.Name + " (for extractor)";

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###",
                         "###");
        }
    }
}