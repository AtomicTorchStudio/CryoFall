namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.InputListeners;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ClientComponents.UI;
    using AtomicTorch.CBND.CoreMod.ClientOptions;
    using AtomicTorch.CBND.CoreMod.ClientOptions.Video;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Physics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.DebugTools;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    [PrepareOrder(afterType: typeof(BootstrapperClientCore))]
    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class BootstrapperClientCoreUI : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            var scene = Api.Client.Scene;

            ClientComponentDebugGrid.Instance.Refresh();
            ClientDebugGuidesManager.Instance.Refresh();
            ClientComponentPhysicsSpaceVisualizer.Init();

            scene.CreateSceneObject("FPS Counter")
                 .AddComponent<ClientComponentPerformanceIndicatorsManager>();

            scene.CreateSceneObject("Console manager")
                 .AddComponent<ClientComponentConsoleErrorsWatcher>();

            ClientInputContext.Start("Console toggle")
                              .HandleButtonDown(
                                  GameButton.ToggleDeveloperConsole,
                                  ConsoleControl.Toggle,
                                  evenIfHandled: true);

            ClientInputContext.Start("Debug tools toggle")
                              .HandleButtonDown(
                                  GameButton.ToggleDebugToolsOverlay,
                                  DebugToolsOverlay.Toggle);

            ClientInputContext.Start("Full screen toggle")
                              .HandleButtonDown(
                                  GameButton.ToggleFullscreen,
                                  () =>
                                  {
                                      var option = ClientOptionsManager.GetOption<VideoOptionScreenMode>();
                                      option.CurrentValue =
                                          option.CurrentValue != VideoOptionScreenMode.ScreenMode.Fullscreen
                                              ? VideoOptionScreenMode.ScreenMode.Fullscreen
                                              : VideoOptionScreenMode.ScreenMode.Windowed;

                                      option.Category.ApplyAndSave();
                                  },
                                  evenIfHandled: true);

            ClientInputContext.Start("Main menu overlay")
                              .HandleButtonDown(
                                  GameButton.CancelOrClose,
                                  MainMenuOverlay.Toggle);

            ClientInputContext.Start("Capture screenshot")
                              .HandleButtonDown(
                                  GameButton.CaptureScreenshot,
                                  () =>
                                  {
                                      if (Client.SteamApi.IsSteamClient
                                          && Client.Input.IsKeyDown(InputKey.F12, evenIfHandled: true))
                                      {
                                          // The default screenshot key (F12) was pressed while playing from Steam Client.
                                          // It will trigger a screenshot by Steam Client (if it using the default key binding).
                                          // Ignore the in-game screenshot functionality to avoid any issues with Steam Client.
                                          return;
                                      }

                                      Client.Rendering.CaptureScreenshot();
                                      Api.Client.Audio.PlayOneShot(new SoundResource("UI/Notifications/Screenshot"));
                                  },
                                  evenIfHandled: true);

            ScreenshotNotification.InitializeScreenshotOverlaySystem();
        }
    }
}