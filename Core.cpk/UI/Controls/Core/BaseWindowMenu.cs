namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BaseWindowMenu : BaseUserControlWithWindow, IMenu
    {
        public event Action IsOpenedChanged;

        public void Toggle()
        {
            this.Window.Toggle();
        }

        void IMenu.InitMenu()
        {
            Api.Client.UI.LayoutRootChildren.Add(this);
            this.InitMenu();
        }

        protected sealed override void DisposeControl()
        {
            Api.Client.UI.LayoutRootChildren.Remove(this);
            this.DisposeMenu();
        }

        protected virtual void DisposeMenu()
        {
        }

        protected sealed override void InitControlWithWindow()
        {
            this.Window.IsCached = true;
        }

        protected virtual void InitMenu()
        {
        }

        protected override void WindowClosed()
        {
            this.Notify();
        }

        protected override void WindowClosing()
        {
            this.Notify();
        }

        protected override void WindowOpened()
        {
            this.Notify();
        }

        protected override void WindowOpening()
        {
            this.Notify();
        }

        private void Notify()
        {
            this.IsOpenedChanged?.Invoke();
        }
    }
}