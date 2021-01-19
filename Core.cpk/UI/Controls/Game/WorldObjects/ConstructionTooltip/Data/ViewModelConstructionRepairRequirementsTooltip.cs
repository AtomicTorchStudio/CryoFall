namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ConstructionTooltip.Data
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelConstructionRepairRequirementsTooltip : BaseViewModelConstructionRequirementsTooltip
    {
        private readonly StaticObjectPublicState objectPublicState;

        private readonly IProtoObjectStructure protoStructure;

        public ViewModelConstructionRepairRequirementsTooltip(StaticObjectPublicState objectPublicState)
        {
            this.objectPublicState = objectPublicState;

            if (objectPublicState is null)
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
                this.protoStructure = (IProtoObjectStructure)this.objectPublicState.GameObject.ProtoGameObject;
                this.objectPublicState.ClientSubscribe(
                    _ => _.StructurePointsCurrent,
                    newStructurePoints => this.UpdateStageCountRemains(),
                    this);

                this.UpdateStageCountRemains();
            }
        }

        public override string ActionTitle => CoreStrings.Action_Repair;

        public override IReadOnlyList<ProtoItemWithCount> StageRequiredItems
            => this.protoStructure.ConfigRepair.StageRequiredItems;

        private void UpdateStageCountRemains()
        {
            this.StageCountRemains = this.CalculateStagesCount(
                this.protoStructure.ConfigRepair,
                this.objectPublicState.StructurePointsCurrent,
                this.protoStructure.StructurePointsMax,
                isAllowTolerance: false);
        }
    }
}