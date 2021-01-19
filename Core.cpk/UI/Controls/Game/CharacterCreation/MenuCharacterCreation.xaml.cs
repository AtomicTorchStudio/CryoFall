namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.CharacterCreation
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterStyle;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuCharacterCreation : BaseUserControl
    {
        private static ClientInputContext inputContext;

        private static MenuCharacterCreation instance;

        private static bool isClosed = true;

        public static void Open()
        {
            if (!isClosed)
            {
                return;
            }

            isClosed = false;
            var menu = new MenuCharacterCreation();
            instance = menu;

            Api.Client.UI.LayoutRootChildren.Add(instance);

            Menu.CloseAll();

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            inputContext = ClientInputContext.Start("Character creation menu - intercept all other input")
                                             .HandleAll(
                                                 () =>
                                                 {
                                                     if (ClientInputManager.IsButtonDown(GameButton.CancelOrClose))
                                                     {
                                                         MainMenuOverlay.Toggle();
                                                     }

                                                     ClientInputManager.ConsumeAllButtons();
                                                 });
        }

        public static void Reset()
        {
            if (isClosed)
            {
                return;
            }

            isClosed = true;
            instance?.RemoveControl();
        }

        protected override void InitControl()
        {
            // special hack for NoesisGUI animation completed event
            this.Tag = this;

            var characterCustomizationControl = this.GetByName<CharacterCustomizationControl>(
                "CharacterCustomizationControl");

            characterCustomizationControl.CallbackClose = this.CloseMenu;
        }

        private async void CloseMenu((CharacterHumanFaceStyle style, bool isMale) result)
        {
            // show splash screen
            LoadingSplashScreenManager.Show("Character created", displayStructureInfos: false);
            await LoadingSplashScreenManager.WaitShownAsync();

            // select the style only now, when the loading splash is displayed,
            // so there is no stuttering of the loading splash screen animation
            CharacterStyleSystem.ClientChangeStyle(result.style, result.isMale);

            this.RemoveControl();

            // allow hiding after a short delay (it will still check whether everything is loaded)
            ClientTimersSystem.AddAction(
                delaySeconds: 0.25 + Math.Min(1, Api.Client.CurrentGame.PingGameSeconds),
                LoadingSplashScreenManager.Hide);
        }

        private void RemoveControl()
        {
            if (instance == this)
            {
                instance = null;
            }

            Api.Client.UI.LayoutRootChildren.Remove(this);
            inputContext?.Stop();
            inputContext = null;
        }
    }
}