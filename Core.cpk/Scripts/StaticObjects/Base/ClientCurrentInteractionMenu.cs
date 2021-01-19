namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;

    public class ClientCurrentInteractionMenu : IMenu
    {
        private static readonly ClientCurrentInteractionMenu MenuInstance;

        private static readonly Menu MenuWrapperInstance;

        private static BaseUserControlWithWindow currentMenuWindow;

        private static bool isOpened;

        static ClientCurrentInteractionMenu()
        {
            MenuInstance = new ClientCurrentInteractionMenu();
            MenuWrapperInstance = Menu.Register(MenuInstance);
        }

        public event Action IsOpenedChanged;

        public bool IsOpened => currentMenuWindow is not null;

        public static void Open()
        {
            MenuWrapperInstance.ToggleCurrentMenu();
        }

        public static void RegisterMenuWindow(BaseUserControlWithWindow menuWindow)
        {
            TryCloseCurrentMenu();
            currentMenuWindow = menuWindow;

            menuWindow.EventWindowClosing += CloseHandler;
            menuWindow.EventWindowClosed += CloseHandler;

            void CloseHandler()
            {
                menuWindow.EventWindowClosing -= CloseHandler;
                menuWindow.EventWindowClosed -= CloseHandler;

                if (ReferenceEquals(currentMenuWindow, menuWindow))
                {
                    currentMenuWindow = null;
                    isOpened = false;
                    MenuInstance.IsOpenedChanged?.Invoke();
                }
            }
        }

        public static void TryCloseCurrentMenu()
        {
            currentMenuWindow?.CloseWindow();
        }

        public void Dispose()
        {
        }

        public void InitMenu()
        {
        }

        public void Toggle()
        {
            if (isOpened)
            {
                currentMenuWindow.CloseWindow();
                return;
            }

            isOpened = true;
            currentMenuWindow.OpenWindow();
            this.IsOpenedChanged?.Invoke();
        }
    }
}