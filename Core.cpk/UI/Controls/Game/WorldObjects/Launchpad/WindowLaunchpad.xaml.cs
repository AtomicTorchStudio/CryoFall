namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Launchpad
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Launchpad.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowLaunchpad : BaseUserControlWithWindow
    {
        private static WindowLaunchpad instance;

        private IStaticWorldObject worldObject;

        public static WindowLaunchpad Open(IStaticWorldObject worldObject)
        {
            if (instance is not null
                && instance.worldObject == worldObject)
            {
                return instance;
            }

            var window = new WindowLaunchpad();
            instance = window;
            window.worldObject = worldObject;
            Api.Client.UI.LayoutRootChildren.Add(window);
            return instance;
        }

        protected override void OnLoaded()
        {
            this.Window.DataContext = new ViewModelWindowLaunchpad(this.worldObject);
        }

        protected override void OnUnloaded()
        {
            if (instance == this)
            {
                instance = null;
            }

            ((ViewModelWindowLaunchpad)this.Window.DataContext).Dispose();
            this.DataContext = null;
        }
    }
}