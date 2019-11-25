namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.LearningPointsNotifications
{
    using System;
    using System.Windows.Controls;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDLearningPointsNotificationsPanelControl : BaseUserControl
    {
        private const int MaxNotificationsToDisplay = 5;

        private const double NotificationHideDelaySeconds = 7.5;

        private static HUDLearningPointsNotificationsPanelControl instance;

        private UIElementCollection stackPanelChildren;

        public static void Show(int deltaCount, ushort currentLearningPoints)
        {
            instance.ShowInternal(deltaCount, currentLearningPoints);
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
                var control = (HUDLearningPointsNotificationControl)this.stackPanelChildren[index];
                control.Hide(quick: true);
            }
        }

        private void ShowInternal(int deltaCount, ushort currentLearningPoints)
        {
            if (deltaCount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaCount));
            }

            var notificationControl = HUDLearningPointsNotificationControl.Create(deltaCount, currentLearningPoints);
            this.stackPanelChildren.Add(notificationControl);

            // hide after delay
            ClientTimersSystem.AddAction(
                NotificationHideDelaySeconds,
                () => notificationControl.Hide(quick: false));

            this.HideOldNotificationsIfTooManyDisplayed();
        }
    }
}