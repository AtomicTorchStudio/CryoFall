namespace AtomicTorch.CBND.CoreMod.ClientComponents.Input
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using JetBrains.Annotations;

    public static partial class ClientInputManager
    {
        [UsedImplicitly]
        private class ClientComponentInputManagerUpdater : ClientComponent
        {
            public override void Update(double deltaTime)
            {
                var loadingScreenState = LoadingSplashScreenManager.Instance.CurrentState;
                if (loadingScreenState == LoadingSplashScreenState.Shown
                    || loadingScreenState == LoadingSplashScreenState.Showing)
                {
                    // don't process input while the loading screen is shown or showing
                    // (except for the console)
                    if (IsButtonDown(GameButton.ToggleDeveloperConsole,
                                     evenIfHandled: true))
                    {
                        ConsoleControl.Toggle();
                    }

                    // (except for the main menu)
                    if (IsButtonDown(GameButton.CancelOrClose))
                    {
                        MainMenuOverlay.Toggle();
                    }

                    return;
                }

                // acquire frozen list of contexts
                var contexts = ClientInputContext.CurrentContexts.FrozenList;

                // iterate it in the reverse order
                for (var index = contexts.Count - 1; index >= 0; index--)
                {
                    var context = contexts[index];
                    context.Update();
                }
            }
        }
    }
}