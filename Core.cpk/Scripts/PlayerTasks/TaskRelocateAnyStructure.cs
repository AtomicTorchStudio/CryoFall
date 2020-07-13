namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskRelocateAnyStructure : BasePlayerTaskWithDefaultState
    {
        public new const string Description = "Relocate any structure";

        private TaskRelocateAnyStructure() : base(Description)
        {
        }

        public override bool IsReversible => false;

        public static TaskRelocateAnyStructure Require()
        {
            return new TaskRelocateAnyStructure();
        }

        public override ITextureResource ClientCreateIcon()
        {
            // a toolbox is used for construction and relocation
            return Api.GetProtoEntity<ItemToolboxT1>().Icon;
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            // cannot complete on its own, player should perform an action
            return false;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                ConstructionRelocationSystem.ServerStructureRelocated += this.ServerStructureRelocatedHandler;
            }
            else
            {
                ConstructionRelocationSystem.ServerStructureRelocated -= this.ServerStructureRelocatedHandler;
            }
        }

        private void ServerStructureRelocatedHandler(ICharacter character, IStaticWorldObject staticWorldObject)
        {
            var context = this.GetActiveContext(character, out _);
            context?.SetCompleted();
        }
    }
}