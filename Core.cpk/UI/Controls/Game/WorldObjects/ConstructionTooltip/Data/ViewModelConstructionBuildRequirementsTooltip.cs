namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ConstructionTooltip.Data
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelConstructionBuildRequirementsTooltip : BaseViewModelConstructionRequirementsTooltip
    {
        private readonly ConstructionSitePublicState constructionSitePublicState;

        private readonly IProtoObjectStructure protoStructure;

        public ViewModelConstructionBuildRequirementsTooltip(
            ConstructionSitePublicState constructionSitePublicState)
        {
            this.constructionSitePublicState = constructionSitePublicState;

            if (constructionSitePublicState == null)
            {
                if (!IsDesignTime)
                {
                    throw new Exception("No construction site server public state provided in game");
                }

                // fake data
                this.protoStructure = new ObjectFloorWood();
            }
            else
            {
                // actual data
                this.protoStructure = this.constructionSitePublicState.ConstructionProto;
                this.constructionSitePublicState.ClientSubscribe(
                    _ => _.StructurePointsCurrent,
                    newStructurePoints => this.UpdateStageCountRemains(),
                    this);

                this.UpdateStageCountRemains();
            }
        }

        public override IReadOnlyList<ProtoItemWithCount> StageRequiredItems
            => this.protoStructure.ConfigBuild.StageRequiredItems;

        public override string ActionTitle => CoreStrings.Action_Build;
        
        private void UpdateStageCountRemains()
        {
            this.StageCountRemains = this.CalculateStagesCount(
                this.protoStructure.ConfigBuild,
                this.constructionSitePublicState.StructurePointsCurrent,
                this.protoStructure.StructurePointsMaxForConstructionSite,
                isAllowTolerance: true);
        }
    }
}