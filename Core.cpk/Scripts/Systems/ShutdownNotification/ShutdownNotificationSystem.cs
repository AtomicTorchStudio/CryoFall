namespace AtomicTorch.CBND.CoreMod.Systems.ShutdownNotification
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class ShutdownNotificationSystem : ProtoSystem<ShutdownNotificationSystem>
    {
        // {0} is duration, {1} is the shutdown reason
        public const string NotificationShutdown_MessageFormat
            = @"Server will shut down in {0}.
  [br]
  [br]Reason:
  [br]{1}
  [br]
  [br]Please find a safe spot and disconnect, otherwise you will be disconnected automatically on timeout.";

        public const string NotificationShutdown_Title = "Shutdown";

        private HudNotificationControl notification;

        private string shutdownReasonMessage;

        private double shutdownServerTime;

        public override string Name => "Shutdown notification system";

        protected override void PrepareSystem()
        {
            base.PrepareSystem();

            if (IsServer)
            {
                Server.Core.ShutdownNotification += this.ShutdownNotificationHandler;
            }
        }

        private void ClientRemote_ShutdownNotification(
            string shutdownReasonMessage,
            double shutdownServerTime)
        {
            this.notification?.Hide(quick: true); // hide already existing notification

            this.shutdownReasonMessage = shutdownReasonMessage;
            this.shutdownServerTime = shutdownServerTime;

            Logger.Important(string.Format(NotificationShutdown_MessageFormat,
                                           ClientTimeFormatHelper.FormatTimeDuration(this.GetSecondsRemains()),
                                           shutdownReasonMessage));

            this.notification = NotificationSystem.ClientShowNotification(
                title: NotificationShutdown_Title,
                message: this.GetShutdownMessage(),
                color: NotificationColor.Bad,
                autoHide: false);

            this.UpdateMessage();
        }

        private uint GetSecondsRemains()
        {
            var timeRemains = Math.Max(0, this.shutdownServerTime - Client.CurrentGame.ServerFrameTimeRounded);
            var secondsRemains = (uint)Math.Round(timeRemains, MidpointRounding.AwayFromZero);
            return secondsRemains;
        }

        private string GetShutdownMessage()
        {
            var secondsRemains = this.GetSecondsRemains();
            return string.Format(NotificationShutdown_MessageFormat,
                                 ClientTimeFormatHelper.FormatTimeDuration(secondsRemains),
                                 this.shutdownReasonMessage);
        }

        private void ShutdownNotificationHandler(string shutdownReasonMessage, double shutdownServerTime)
        {
            this.CallClient(
                Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                _ => _.ClientRemote_ShutdownNotification(shutdownReasonMessage, shutdownServerTime));
        }

        private void UpdateMessage()
        {
            if (this.notification.IsHiding)
            {
                return;
            }

            var secondsRemains = this.GetSecondsRemains();
            if (secondsRemains <= 0)
            {
                return;
            }

            this.notification.Message = this.GetShutdownMessage();
            ClientTimersSystem.AddAction(1, this.UpdateMessage);
        }
    }
}