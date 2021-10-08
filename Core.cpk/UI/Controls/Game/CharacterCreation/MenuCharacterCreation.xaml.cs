namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.CharacterCreation
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterCreation;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.CharacterCreation.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuCharacterCreation : BaseUserControl
    {
        private static ClientInputContext inputContext;

        private static MenuCharacterCreation instance;

        private static bool isClosed = true;

        private ViewModelMenuCharacterCreation viewModel;

        public static void HideIfCharacterCreated()
        {
            if (instance is not null
                && CharacterCreationSystem.SharedIsCharacterCreated(ClientCurrentCharacterHelper.Character))
            {
                instance.RemoveControl();
            }
        }

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
        }

        protected override void OnLoaded()
        {
            var characterCustomizationControl = this.GetByName<CharacterCustomizationControl>(
                "CharacterCustomizationControl");

            this.DataContext = this.viewModel =
                                   new ViewModelMenuCharacterCreation(characterCustomizationControl,
                                                                      closeCallback: this.RemoveControl);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
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