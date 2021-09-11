namespace AtomicTorch.CBND.CoreMod.Systems.ShutdownNotification
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
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

        private static double sharedServerShutdownServerTime;

        private static string sharedShutdownReasonMessage;

        private HudNotificationControl clientNotification;

        public override string Name => "Shutdown notification system";

        protected override void PrepareSystem()
        {
            base.PrepareSystem();

            if (IsServer)
            {
                Server.Core.ShutdownNotification += this.ServerShutdownNotificationHandler;
                Server.Characters.PlayerOnlineStateChanged += this.ServerCharactersPlayerOnlineStateChangedHandler;
            }
        }

        private void ClientRemote_ShutdownNotification(
            string message,
            double serverTime)
        {
            this.clientNotification?.Hide(quick: true); // hide already existing notification

            sharedShutdownReasonMessage = message;
            sharedServerShutdownServerTime = serverTime;

            Logger.Important(
                string.Format(NotificationShutdown_MessageFormat,
                              ClientTimeFormatHelper.FormatTimeDuration(this.SharedGetSecondsRemains()),
                              sharedShutdownReasonMessage));

            this.clientNotification = NotificationSystem.ClientShowNotification(
                title: NotificationShutdown_Title,
                message: this.SharedGetShutdownMessage(),
                color: NotificationColor.Bad,
                autoHide: false);

            this.ClientUpdateMessage();
        }

        private void ClientUpdateMessage()
        {
            if (this.clientNotification.IsHiding)
            {
                return;
            }

            var secondsRemains = this.SharedGetSecondsRemains();
            if (secondsRemains <= 0)
            {
                return;
            }

            this.clientNotification.Message = this.SharedGetShutdownMessage();
            ClientTimersSystem.AddAction(1, this.ClientUpdateMessage);
        }

        private void ServerCharactersPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            if (!isOnline)
            {
                return;
            }

            if (sharedServerShutdownServerTime <= 0)
            {
                // no shutdown initialized
                return;
            }

            // notify the connected player about the upcoming server shutdown
            this.CallClient(
                Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                _ => _.ClientRemote_ShutdownNotification(sharedShutdownReasonMessage,
                                                         sharedServerShutdownServerTime));
        }

        private void ServerShutdownNotificationHandler(string message, double time)
        {
            sharedShutdownReasonMessage = message;
            sharedServerShutdownServerTime = time;

            this.CallClient(
                Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                _ => _.ClientRemote_ShutdownNotification(sharedShutdownReasonMessage,
                                                         sharedServerShutdownServerTime));
        }

        private uint SharedGetSecondsRemains()
        {
            var timeRemains = Math.Max(0,
                                       sharedServerShutdownServerTime - Client.CurrentGame.ServerFrameTimeRounded);
            var secondsRemains = (uint)Math.Round(timeRemains, MidpointRounding.AwayFromZero);
            return secondsRemains;
        }

        private string SharedGetShutdownMessage()
        {
            var secondsRemains = this.SharedGetSecondsRemains();
            return string.Format(NotificationShutdown_MessageFormat,
                                 ClientTimeFormatHelper.FormatTimeDuration(secondsRemains),
                                 sharedShutdownReasonMessage);
        }
    }
}