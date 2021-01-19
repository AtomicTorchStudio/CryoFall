namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HudNotificationsPanelControl : BaseUserControl
    {
        private const int MaxNotificationsToDisplay = 7;

        private const double NotificationHideDelaySeconds = 7.5;

        private static HudNotificationsPanelControl instance;

        private UIElementCollection stackPanelChildren;

        public static void ShowNotificationControl
            <TNotificationControl>(TNotificationControl notificationControl)
            where TNotificationControl : UIElement, IHudNotificationControl
        {
            var panelControl = instance;
            if (notificationControl.IsAutoHide)
            {
                panelControl.HideSimilarNotifications(notificationControl);
            }

            panelControl.stackPanelChildren.Add(notificationControl);

            if (notificationControl.IsAutoHide)
            {
                // hide the notification control after delay
                ClientTimersSystem.AddAction(
                    NotificationHideDelaySeconds,
                    () =>
                    {
                        if (notificationControl.IsAutoHide) // still configured as auto hide
                        {
                            notificationControl.Hide(quick: false);
                        }
                    });
            }

            panelControl.HideOldNotificationsIfTooManyDisplayed();
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
                var control = (IHudNotificationControl)this.stackPanelChildren[index];
                if (control.IsAutoHide)
                {
                    control.Hide(quick: true);
                }
            }
        }

        private void HideSimilarNotifications(IHudNotificationControl notificationControl)
        {
            for (var index = 0; index < this.stackPanelChildren.Count; index++)
            {
                var control = (IHudNotificationControl)this.stackPanelChildren[index];
                if (control.IsSame(notificationControl))
                {
                    control.Hide(quick: true);
                }
            }
        }
    }
}