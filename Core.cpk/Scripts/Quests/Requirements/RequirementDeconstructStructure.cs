namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Systems.Deconstruction;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementDeconstructStructure : QuestRequirementWithList<IProtoObjectStructure>
    {
        // no need to localize it now, we're using this requirement to deconstruct any building
        [NotLocalizable]
        public const string DescriptionFormat = "Deconstruct: {0}";

        public RequirementDeconstructStructure(
            IReadOnlyList<IProtoObjectStructure> list,
            ushort count,
            string description)
            : base(list, count, description)
        {
        }

        public override bool IsReversible => false;

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ListNames);

        public static RequirementDeconstructStructure Require<TProtoStructure>(
            ushort count = 1,
            string description = null)
            where TProtoStructure : class, IProtoObjectStructure
        {
            var list = Api.FindProtoEntities<TProtoStructure>();
            return new RequirementDeconstructStructure(list, count, description);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                DeconstructionSystem.ServerStructureDeconstructed += this.StructureDeconstructedHandler;
            }
            else
            {
                DeconstructionSystem.ServerStructureDeconstructed -= this.StructureDeconstructedHandler;
            }
        }

        private void StructureDeconstructedHandler(DeconstructionActionState deconstructionActionState)
        {
            var character = deconstructionActionState.Character;

            var context = this.GetActiveContext(character, out var state);
            if (context == null)
            {
                return;
            }

            var structure = deconstructionActionState.WorldObject;
            var protoStaticWorldObject = structure.ProtoStaticWorldObject;
            var protoTool = deconstructionActionState.ProtoItemCrowbarTool;

            if (protoTool == null
                || protoStaticWorldObject is ProtoObjectConstructionSite)
            {
                // it's a blueprint and/or deconstructed not by a tool
                return;
            }

            if (!this.List.Contains(protoStaticWorldObject))
            {
                return;
            }

            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            context.Refresh();
        }
    }
}