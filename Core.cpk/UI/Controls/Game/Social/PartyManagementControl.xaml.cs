namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class PartyManagementControl : BaseUserControl
    {
        protected override void InitControl()
        {
            this.DataContext = new ViewModelPartyManagementControl();
        }
    }
}