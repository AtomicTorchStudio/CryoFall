namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Quit
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMenuQuit : BaseViewModel
    {
        public BaseCommand CommandQuit
            => new ActionCommand(() => Client.Core.Quit());
    }
}