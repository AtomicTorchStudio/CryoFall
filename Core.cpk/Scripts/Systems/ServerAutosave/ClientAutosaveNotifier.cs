namespace AtomicTorch.CBND.CoreMod.Systems.ServerAutosave
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientAutosaveNotifier
    {
        [NotLocalizable]
        private const string NotificationMessageBaseText
            = CoreStrings.AutosaveNotification_Content1
              + "[br]"
              + CoreStrings.AutosaveNotification_Content2
              + "[br]"
              + "[br]";

        private static readonly TextureResource TextureResourceIconSave
            = new("FX/Special/IconSave.png");

        private static HudNotificationControl notificationControl;

        private static double? serverSaveScheduledTime;

        private static void ClientSaveFinishedHandler()
        {
            var control = notificationControl;
            if (control is null)
            {
                return;
            }

            notificationControl = null;
            serverSaveScheduledTime = null;

            control.Message = NotificationMessageBaseText
                              + CoreStrings.AutosaveNotification_Completed;

            ClientTimersSystem.AddAction(2,
                                         () => control.Hide(quick: false));
        }

        private static void ClientSaveScheduledHandler(double scheduledTime)
        {
            serverSaveScheduledTime = scheduledTime;
            notificationControl?.Hide(quick: true);
            notificationControl = null;

            notificationControl = NotificationSystem.ClientShowNotification(
                CoreStrings.AutosaveNotification_Title,
                NotificationMessageBaseText,
                NotificationColor.Neutral,
                icon: TextureResourceIconSave,
                autoHide: false);

            UpdateNotificationControlText();
        }

        private static void ConnectionStateChangedHandler()
        {
            notificationControl?.Hide(quick: true);
            notificationControl = null;
        }

        private static void UpdateNotificationControlText()
        {
            if (notificationControl is null
                || serverSaveScheduledTime is null)
            {
                return;
            }

            string message;

            var secondsRemains = serverSaveScheduledTime.Value - Api.Client.CurrentGame.ServerFrameTimeApproximated;
            if (secondsRemains <= 0)
            {
                secondsRemains = 0;
                message = CoreStrings.AutosaveNotification_Saving;
            }
            else
            {
                message = string.Format(CoreStrings.AutosaveNotification_DelayRemains_Format,
                                        ClientTimeFormatHelper.FormatTimeDuration(secondsRemains));
            }

            notificationControl.Message = NotificationMessageBaseText + message;

            if (secondsRemains > 0)
            {
                ClientTimersSystem.AddAction(1, UpdateNotificationControlText);
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                ServerAutosaveSystem.ClientSaveScheduled += ClientSaveScheduledHandler;
                ServerAutosaveSystem.ClientSaveFinished += ClientSaveFinishedHandler;
                Client.CurrentGame.ConnectionStateChanged += ConnectionStateChangedHandler;
            }
        }
    }
}