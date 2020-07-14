namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.LandClaims
{
    public partial class WindowLandClaim : WindowLandClaimBase
    {
        protected override void InitControlWithWindow()
        {
            // TODO: redone this to cached window when NoesisGUI implement proper Storyboard.Completed triggers
            this.Window.IsCached = false;
        }
    }
}