namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.LandClaims
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowLandClaim : WindowLandClaimBase
    {
        private TabControl tabControl;

        protected override void InitControlWithWindow()
        {
            // TODO: redone this to cached window when NoesisGUI implement proper Storyboard.Completed triggers
            this.Window.IsCached = false;

            this.tabControl = this.GetByName<WindowMenuWithInventory>("WindowMenuWithInventory")
                                  .GetByName<TabControl>("TabControl");
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.ViewModel.OnActiveTabChanged = this.ActiveTabChangedHandler;
        }

        private void ActiveTabChangedHandler()
        {
            if (this.ViewModel is null)
            {
                return;
            }

            // NoesisGUI bug workaround to ensure the previously selected tab is clickable 
            // https://www.noesisengine.com/bugs/view.php?id=1751
            this.tabControl.Visibility = Visibility.Collapsed;
            this.tabControl.UpdateLayout();
            this.tabControl.Visibility = Visibility.Visible;
        }
    }
}