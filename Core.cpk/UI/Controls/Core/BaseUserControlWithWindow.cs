namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class BaseUserControlWithWindow : BaseUserControl
    {
        public event Action EventWindowClosed;

        public event Action EventWindowClosing;

        public DialogResult DialogResult => this.Window?.DialogResult
                                            ?? throw new Exception("Don't have a GameWindow instance");

        public bool IsOpened => this.WindowState == GameWindowState.Opened
                                || this.WindowState == GameWindowState.Opening;

        public GameWindow Window { get; private set; }

        public GameWindowState WindowState => this.Window?.State ?? GameWindowState.Closed;

        public void CloseWindow(DialogResult dialogResult = DialogResult.Cancel)
        {
            this.Window.Close(dialogResult);
        }

        public void OpenWindow()
        {
            this.Window.Open();
        }

        protected sealed override void InitControl()
        {
            this.Window = this.FindName<GameWindow>("GameWindow");

            if (this.Window is null)
            {
                var windowHolder = this.FindName<WindowMenuWithInventory>("WindowMenuWithInventory");
                this.Window = windowHolder?.Window ?? throw new Exception("GameWindow not found");
            }

            this.Window.Tag = this;
            this.Window.StateChanged += GameWindowStateChangedHandler;

            this.InitControlWithWindow();

            if (!this.Window.IsCached)
            {
                this.Window.Open();
            }
        }

        protected virtual void InitControlWithWindow()
        {
        }

        protected virtual void WindowClosed()
        {
            if (!this.Window.IsCached)
            {
                Api.Client.UI.LayoutRootChildren.Remove(this);
            }
        }

        protected virtual void WindowClosing()
        {
        }

        protected virtual void WindowOpened()
        {
        }

        protected virtual void WindowOpening()
        {
        }

        private static void GameWindowStateChangedHandler(GameWindow window)
        {
            var control = (BaseUserControlWithWindow)window.Tag;
            switch (window.State)
            {
                case GameWindowState.Opening:
                    control.WindowOpening();
                    break;

                case GameWindowState.Opened:
                    control.WindowOpened();
                    break;

                case GameWindowState.Closing:
                    control.WindowClosing();
                    control.EventWindowClosing?.Invoke();
                    break;

                case GameWindowState.Closed:
                    control.WindowClosed();
                    control.EventWindowClosed?.Invoke();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}