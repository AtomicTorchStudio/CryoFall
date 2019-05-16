namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class DuelModeControl : BaseUserControl
    {
        protected override void InitControl()
        {
            this.DataContext = new ViewModelDuelModeControl();
        }
    }
}