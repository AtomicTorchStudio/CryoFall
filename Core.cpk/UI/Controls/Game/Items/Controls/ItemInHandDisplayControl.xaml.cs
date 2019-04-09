namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class ItemInHandDisplayControl : BaseUserControl
    {
        private static readonly Vector2F DisplayOffset = (10, 10);

        private bool isShown;

        private ItemSlotControl itemSlotControl;

        private Point? lastPosition;

        public ItemInHandDisplayControl()
        {
        }

        public void Hide()
        {
            if (!this.isShown)
            {
                return;
            }

            this.isShown = false;
            Api.Client.UI.LayoutRootChildren.Remove(this);
        }

        public void Show()
        {
            if (this.isShown)
            {
                return;
            }

            this.isShown = true;
            Api.Client.UI.LayoutRootChildren.Add(this);
        }

        protected override void InitControl()
        {
            this.itemSlotControl = (ItemSlotControl)this.Content;
            this.itemSlotControl.IsSelectable = false;
        }

        protected override void OnLoaded()
        {
            ClientComponentUpdateHelper.UpdateCallback += this.UpdatePopupPosition;

            this.Refresh();
            this.UpdatePopupPosition();
        }

        protected override void OnUnloaded()
        {
            ClientComponentUpdateHelper.UpdateCallback -= this.UpdatePopupPosition;
        }

        private void Refresh()
        {
            this.itemSlotControl.Setup(ClientItemsManager.HandContainer, 0);
            this.itemSlotControl.RefreshItem();
        }

        private void UpdatePopupPosition()
        {
            var mousePosition = Mouse.GetPosition(Api.Client.UI.LayoutRoot);
            if (mousePosition == this.lastPosition)
            {
                return;
            }

            this.lastPosition = mousePosition;
            this.Margin = new Thickness(left: mousePosition.X + DisplayOffset.X,
                                        top: mousePosition.Y + DisplayOffset.Y,
                                        right: 0,
                                        bottom: 0);
        }
    }
}