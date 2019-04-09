namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDNotificationsPanelControl : BaseUserControl
    {
        private const int MaxNotificationsToDisplay = 7;

        private const double NotificationHideDelaySeconds = 7.5;

        private static HUDNotificationsPanelControl instance;

        private UIElementCollection stackPanelChildren;

        public static HUDNotificationControl Show(
            string title,
            string message,
            Brush brushBackground,
            Brush brushBorder,
            ITextureResource icon,
            Action onClick,
            bool autoHide,
            SoundResource soundToPlay)
        {
            return instance.ShowInternal(title,
                                         message,
                                         brushBackground,
                                         brushBorder,
                                         icon,
                                         onClick,
                                         autoHide,
                                         soundToPlay);
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
                var control = (HUDNotificationControl)this.stackPanelChildren[index];
                if (control.IsAutoHide)
                {
                    control.Hide(quick: true);
                }
            }
        }

        private void HideSimilarNotifications(HUDNotificationControl notificationControl)
        {
            var count = this.stackPanelChildren.Count;
            for (var index = 0; index < count; index++)
            {
                var control = (HUDNotificationControl)this.stackPanelChildren[index];
                if (control.IsSame(notificationControl))
                {
                    control.Hide(quick: true);
                }
            }
        }

        private HUDNotificationControl ShowInternal(
            string title,
            string message,
            Brush brushBackground,
            Brush brushBorder,
            ITextureResource icon,
            Action onClick,
            bool autoHide,
            SoundResource soundToPlay)
        {
            Api.Logger.Important(
                string.Format(
                    "Showing notification:{0}Title: {1}{0}Message: {2}",
                    Environment.NewLine,
                    title,
                    message));

            var notificationControl = HUDNotificationControl.Create(
                title,
                message,
                brushBackground,
                brushBorder,
                icon,
                onClick,
                autoHide,
                soundToPlay);

            this.HideSimilarNotifications(notificationControl);

            this.stackPanelChildren.Add(notificationControl);

            if (notificationControl.IsAutoHide)
            {
                // hide the notification control after delay
                ClientComponentTimersManager.AddAction(
                    NotificationHideDelaySeconds,
                    () => notificationControl.Hide(quick: false));
            }

            this.HideOldNotificationsIfTooManyDisplayed();

            return notificationControl;
        }
    }
}