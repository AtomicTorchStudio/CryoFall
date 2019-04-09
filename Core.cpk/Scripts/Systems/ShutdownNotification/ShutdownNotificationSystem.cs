namespace AtomicTorch.CBND.CoreMod.Systems.ShutdownNotification
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
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

        public override string Name => "Shutdown notification system";

        protected override void PrepareSystem()
        {
            base.PrepareSystem();

            if (IsServer)
            {
                Server.Core.ShutdownNotification += this.ShutdownNotificationHandler;
            }
        }

        private void ClientRemote_ShutdownNotification(string shutdownReasonMessage, double shutdownServerTime)
        {
            var timeRemains = shutdownServerTime - Client.CurrentGame.ServerFrameTimeRounded;
            var secondsRemains = (uint)Math.Round(timeRemains, MidpointRounding.AwayFromZero);

            Logger.Important(string.Format(NotificationShutdown_MessageFormat,
                                           ClientTimeFormatHelper.FormatTimeDuration(secondsRemains),
                                           shutdownReasonMessage));

            NotificationSystem.ClientShowNotification(
                title: NotificationShutdown_Title,
                message: string.Format(NotificationShutdown_MessageFormat,
                                       ClientTimeFormatHelper.FormatTimeDuration(secondsRemains),
                                       shutdownReasonMessage),
                color: NotificationColor.Bad,
                autoHide: false);
        }

        private void ShutdownNotificationHandler(string shutdownReasonMessage, double shutdownServerTime)
        {
            this.CallClient(
                Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                _ => _.ClientRemote_ShutdownNotification(shutdownReasonMessage, shutdownServerTime));
        }
    }
}