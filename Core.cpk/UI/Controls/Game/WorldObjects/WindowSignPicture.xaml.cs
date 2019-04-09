namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public partial class WindowSignPicture : WindowSignPictureBase
    {
        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.ViewModel.CloseWindowCallback = () => this.Window.Close(DialogResult.OK);
        }
    }
}