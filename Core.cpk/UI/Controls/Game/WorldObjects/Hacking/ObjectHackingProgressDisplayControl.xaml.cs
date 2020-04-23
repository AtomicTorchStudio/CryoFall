namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Hacking
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Hacking.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ObjectHackingProgressDisplayControl : BaseUserControl
    {
        private readonly LootHackingContainerPublicState publicState;

        private ViewModelObjectHackingProgressDisplayControl viewModel;

        public ObjectHackingProgressDisplayControl(LootHackingContainerPublicState publicState)
        {
            this.publicState = publicState;
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel
                                   = new ViewModelObjectHackingProgressDisplayControl(this.publicState);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}