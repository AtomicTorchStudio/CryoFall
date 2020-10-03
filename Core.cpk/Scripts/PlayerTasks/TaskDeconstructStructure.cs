namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Systems.Deconstruction;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskDeconstructStructure : BasePlayerTaskWithListAndCount<IProtoObjectStructure>
    {
        // no need to localize it now, we're using this requirement to deconstruct any building
        [NotLocalizable]
        public const string DescriptionFormat = "Deconstruct: {0}";

        public TaskDeconstructStructure(
            IReadOnlyList<IProtoObjectStructure> list,
            ushort count,
            string description)
            : base(list, count, description)
        {
        }

        public override bool IsReversible => false;

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ListNames);

        public static TaskDeconstructStructure Require<TProtoStructure>(
            ushort count = 1,
            string description = null)
            where TProtoStructure : class, IProtoObjectStructure
        {
            var list = Api.FindProtoEntities<TProtoStructure>();
            return new TaskDeconstructStructure(list, count, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            // crowbar is used for deconstruction so it's fine to use its icon
            return Api.GetProtoEntity<ItemCrowbar>().Icon;
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
            if (context is null)
            {
                return;
            }

            var structure = deconstructionActionState.WorldObject;
            var protoStaticWorldObject = structure.ProtoStaticWorldObject;
            var protoTool = deconstructionActionState.ProtoItemCrowbarTool;

            if (protoTool is null
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