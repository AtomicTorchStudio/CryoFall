namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Demo.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelDemoVersionWelcomeMenu : BaseViewModel
    {
        public BaseCommand CommandBuy
            => new ActionCommand(
                () => Api.Client.SteamApi.OpenBuyGamePage());

        public BaseCommand CommandContinue
            => new ActionCommand(DemoVersionWelcomeMenu.Close);

        public BaseCommand CommandQuit
            => new ActionCommand(Client.Core.Quit);

        public bool IsExpired => Api.Client.MasterServer.DemoIsExpired;
    }
}