namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowContainerHelp : BaseUserControlWithWindow
    {
        public static readonly BaseCommand CommandOpenMenu
            = new ActionCommand(
                () => Api.Client.UI.LayoutRootChildren.Add(
                    new WindowContainerHelp()));

        protected override void InitControlWithWindow()
        {
            base.InitControlWithWindow();
            this.Window.IsCached = false;
        }
    }
}