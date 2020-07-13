namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
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

        private IHudNotificationControl ShowInternal(
            string title,
            string message,
            Brush brushBackground,
            Brush brushBorder,
            ITextureResource icon,
            Action onClick,
            bool autoHide,
            SoundResource soundToPlay,
            bool writeToLog)
        {
            if (writeToLog)
            {
                Api.Logger.Important(
                    string.Format(
                        "Showing notification:{0}Title: {1}{0}Message: {2}",
                        Environment.NewLine,
                        title,
                        message));
            }

            var notificationControl = HudNotificationControl.Create(
                title,
                message,
                brushBackground,
                brushBorder,
                icon,
                onClick,
                autoHide,
                soundToPlay);

            var instance = this;
            if (notificationControl.IsAutoHide)
            {
                instance.HideSimilarNotifications(notificationControl);
            }

            instance.stackPanelChildren.Add(notificationControl);

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

            instance.HideOldNotificationsIfTooManyDisplayed();

            return notificationControl;
        }
    }
}