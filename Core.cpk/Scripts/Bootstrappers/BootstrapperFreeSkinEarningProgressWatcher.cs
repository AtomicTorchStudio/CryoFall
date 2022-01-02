namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BootstrapperFreeSkinEarningProgressWatcher : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            Client.Microtransactions.SkinsDataReceived += SkinsDataReceivedHandler;
        }

        private static void SkinsDataReceivedHandler()
        {
            if (!Client.Microtransactions.AreSkinsSupported
                || !Client.Microtransactions.IsDataReceived)
            {
                WindowFreeSkinProgressCompleted.Hide();
                return;
            }

            var daysRequired = Client.Microtransactions.DaysToUnlockEarnedSkinRequired;
            if (daysRequired == 0)
            {
                WindowFreeSkinProgressCompleted.Hide();
                return;
            }

            if (Client.Microtransactions.DaysToUnlockEarnedSkinPassed < daysRequired)
            {
                WindowFreeSkinProgressCompleted.Hide();
                return;
            }

            // has a free skin to claim
            if (!SkinsMenuOverlay.IsDisplayed
                && !MainMenuOverlay.IsHidden)
            {
                WindowFreeSkinProgressCompleted.Show();
            }
        }
    }
}