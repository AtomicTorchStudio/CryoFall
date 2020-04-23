namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Hacking.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelObjectHackingProgressDisplayControl : BaseViewModel
    {
        private readonly LootHackingContainerPublicState publicState;

        public ViewModelObjectHackingProgressDisplayControl(LootHackingContainerPublicState publicState)
        {
            this.publicState = publicState;
            this.publicState.ClientSubscribe(_ => _.HackingProgressPercent,
                                             _ => this.NotifyPropertyChanged(nameof(this.HackingProgress)),
                                             this);
        }

        public byte HackingProgress => (byte)this.publicState.HackingProgressPercent;
    }
}