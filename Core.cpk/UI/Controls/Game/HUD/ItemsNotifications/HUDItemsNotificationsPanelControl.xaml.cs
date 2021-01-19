namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.ItemsNotifications
{
    using System;
    using System.Windows.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDItemsNotificationsPanelControl : BaseUserControl
    {
        private const int MaxNotificationsToDisplay = 7;

        private const double NotificationHideDelaySeconds = 7.5;

        private static HUDItemsNotificationsPanelControl instance;

        private UIElementCollection stackPanelChildren;

        public static HUDItemNotificationControl Show(IProtoItem protoItem, int deltaCount)
        {
            return instance?.ShowInternal(protoItem, deltaCount);
        }

        protected override void InitControl()
        {
            instance = this;
            this.stackPanelChildren = this.GetByName<StackPanel>("StackPanel").Children;

            if (!IsDesignTime)
            {
                this.stackPanelChildren.Clear();
            }
        }

        private void HideOldNotificationsIfTooManyDisplayed()
        {
            var countToHide = this.stackPanelChildren.Count - MaxNotificationsToDisplay;
            for (var index = 0; index < countToHide; index++)
            {
                var control = (HUDItemNotificationControl)this.stackPanelChildren[index];
                control.Hide(quick: true);
            }
        }

        private HUDItemNotificationControl ShowInternal(IProtoItem protoItem, int deltaCount)
        {
            if (protoItem is null)
            {
                throw new ArgumentNullException(nameof(protoItem));
            }

            if (deltaCount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaCount));
            }

            var notificationControl = HUDItemNotificationControl.Create(protoItem, deltaCount);
            this.stackPanelChildren.Add(notificationControl);

            // hide after delay
            ClientTimersSystem.AddAction(
                NotificationHideDelaySeconds,
                () => notificationControl.Hide(quick: false));

            this.HideOldNotificationsIfTooManyDisplayed();
            return notificationControl;
        }
    }
}