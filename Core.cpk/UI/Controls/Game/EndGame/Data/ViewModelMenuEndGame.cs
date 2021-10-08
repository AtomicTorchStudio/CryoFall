namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.EndGame.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.CharacterRespawn;
    using AtomicTorch.CBND.CoreMod.Systems.NewGamePlus;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMenuEndGame : BaseViewModel
    {
        public bool CanStartNewGamePlus
            => !Api.IsEditor
               && Client.CurrentGame.ServerInfo.ServerAddress.IsLocalServer;

        public BaseCommand CommandEject
            => new ActionCommand(ExecuteCommandEject);

        public BaseCommand CommandNewGamePlus
            => new ActionCommand(ExecuteCommandNewGamePlus);

        public BaseCommand CommandReturnToMainMenu
            => new ActionCommand(ExecuteCommandReturnToMainMenu);

        public bool IsEditor => Api.IsEditor;

        private static void ExecuteCommandEject()
        {
            ToolTipServiceExtend.CloseOpenedTooltip();
            CharacterRespawnSystem.Instance.ClientRequestRespawnInWorld();
        }

        private static void ExecuteCommandNewGamePlus()
        {
            NewGamePlusSystem.ClientStartNewGamePlus();
        }

        private static void ExecuteCommandReturnToMainMenu()
        {
            Client.CurrentGame.Disconnect();
            ViewModelMainMenuOverlay.Instance.IsHomeTabSelected = true;
        }
    }
}