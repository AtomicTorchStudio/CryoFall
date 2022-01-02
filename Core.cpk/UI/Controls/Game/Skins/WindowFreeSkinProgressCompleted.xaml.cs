namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowFreeSkinProgressCompleted : BaseUserControlWithWindow
    {
        private static WindowFreeSkinProgressCompleted instance;

        private WindowFreeSkinProgressCompleted()
        {
        }

        public static void Show()
        {
            var window = new WindowFreeSkinProgressCompleted();
            Api.Client.UI.LayoutRootChildren.Add(window);
            window.OpenWindow();
        }

        protected override void OnLoaded()
        {
            instance = this;
        }

        protected override void OnUnloaded()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public static void Hide()
        {
            instance?.Window.Close(DialogResult.Cancel);
        }
    }
}