namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ConstructionSitePublicState : StaticObjectPublicState
    {
        [SyncToClient]
        public IProtoObjectStructure ConstructionProto { get; set; }

        [TempOnly]
        public ICharacter LastBuildActionDoneByCharacter { get; set; }

        public void Setup(IProtoObjectStructure protoStructure)
        {
            this.ConstructionProto = protoStructure;

            // set structure points to match first build stage
            this.StructurePointsCurrent = protoStructure.StructurePointsMaxForConstructionSite
                                          / protoStructure.ConfigBuild.StagesCount;
            if (this.StructurePointsCurrent < 1)
            {
                this.StructurePointsCurrent = 1;
            }
        }
    }
}