namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionDiplomacyEntryControl : BaseUserControl
    {
        protected override void OnLoaded()
        {
            this.MouseUp += this.MouseUpHandler;
        }

        protected override void OnUnloaded()
        {
            this.MouseUp -= this.MouseUpHandler;
        }

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (!this.IsHitTestVisible)
            {
                return;
            }

            var viewModel = (ViewModelFactionDiplomacyStatusEntry)this.DataContext;
            var clanTag = viewModel.ClanTag;

            ClientFactionContextMenu.Show(clanTag,
                                          addShowFactionInformationMenuEntry: true);
        }
    }
}