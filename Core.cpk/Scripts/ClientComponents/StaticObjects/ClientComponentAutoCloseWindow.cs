namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Windows;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentAutoCloseWindow : ClientComponent
    {
        private Func<bool> closeCheckFunc;

        private WindowContainerExchange window;

        public void Setup(WindowContainerExchange setWindow, Func<bool> closeCheckFunc)
        {
            this.window = setWindow;
            this.closeCheckFunc = closeCheckFunc;
        }

        public override void Update(double deltaTime)
        {
            if (!this.window.IsOpened)
            {
                // player closed the window?
                this.Destroy();
                return;
            }

            if (this.closeCheckFunc())
            {
                this.window.Close();
                this.Destroy();
            }
        }
    }
}