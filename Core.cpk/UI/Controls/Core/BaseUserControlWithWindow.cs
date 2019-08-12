namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class BaseUserControlWithWindow : BaseUserControl
    {
        public GameWindow Window { get; private set; }

        public event Action EventWindowClosed;

        public event Action EventWindowClosing;

        public DialogResult DialogResult => this.Window?.DialogResult
                                            ?? throw new Exception("Don't have a GameWindow instance");

        public bool IsOpened => this.WindowState == GameWindowState.Opened
                                || this.WindowState == GameWindowState.Opening;

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
            var windowHolder = this.FindName<WindowMenuWithInventory>("WindowMenuWithInventory");
            this.Window = windowHolder?.Window ?? this.GetByName<GameWindow>("GameWindow");

            // TODO: fix possible memory leak
            this.Window.StateChanged += this.GameWindowStateChangedHandler;

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

            //this.EventWindowClosed?.Invoke();
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

        private void GameWindowStateChangedHandler(GameWindow window)
        {
            switch (window.State)
            {
                case GameWindowState.Opening:
                    this.WindowOpening();
                    break;

                case GameWindowState.Opened:
                    this.WindowOpened();
                    break;

                case GameWindowState.Closing:
                    this.WindowClosing();
                    this.EventWindowClosing?.Invoke();
                    break;

                case GameWindowState.Closed:
                    this.WindowClosed();
                    this.EventWindowClosed?.Invoke();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}