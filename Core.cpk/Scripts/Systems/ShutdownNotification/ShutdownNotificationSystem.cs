namespace AtomicTorch.CBND.CoreMod.Systems.ShutdownNotification
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ShutdownNotificationSystem : ProtoSystem<ShutdownNotificationSystem>
    {
        public const string Message_ServerMaintenance =
            "Server maintenance.";

        public const string Message_ServerRatesChange =
            "Server rates will be changed.";

        public const string Message_ServerReboot =
            "Server reboot";

        public const string Message_ServerUpdate =
            "Server update to the latest version.";

        // wipe in this context - complete server wipe and world reset (basically, a new game)
        public const string Message_ServerWipe =
            "Server wipe.";

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

        private static string sharedShutdownReasonMessageRaw;

        private HudNotificationControl clientNotification;

        public static string LastDisconnectReasonMessage
        {
            get
            {
                if (Api.Client.CurrentGame.ConnectionState == ConnectionState.Connected)
                {
                    // currently connected
                    return null;
                }

                return GetFormattedShutdownReasonMessage();
            }
        }

        public override string Name => "Shutdown notification system";

        protected override void PrepareSystem()
        {
            base.PrepareSystem();

            if (IsServer)
            {
                Server.Core.ShutdownNotification += this.ServerShutdownNotificationHandler;
                Server.Characters.PlayerOnlineStateChanged += this.ServerCharactersPlayerOnlineStateChangedHandler;
            }
            else
            {
                Client.CurrentGame.ConnectionStateChanged += this.ClientCurrentGameConnectionStateChangedHandler;
            }
        }

        private static void ExtractHeader(
            ref string shutdownReason,
            out string header)
        {
            var indexOfSpace = shutdownReason.IndexOf(' ');
            var firstWord = indexOfSpace > 0
                                ? shutdownReason.Substring(0, indexOfSpace)
                                : shutdownReason;

            header = firstWord.ToLowerInvariant() switch
            {
                "update"      => Message_ServerUpdate,
                "wipe"        => Message_ServerWipe,
                "maintenance" => Message_ServerMaintenance,
                "relocation"  => Message_ServerMaintenance,
                "rate"        => Message_ServerRatesChange,
                "rates"       => Message_ServerRatesChange,
                "reboot"      => Message_ServerReboot,
                _             => null
            };

            if (header != null)
            {
                shutdownReason = shutdownReason.Substring(
                                                   indexOfSpace > 0
                                                       ? firstWord.Length + 1
                                                       : firstWord.Length)
                                               .TrimStart();
            }
        }

        private static string GetFormattedShutdownReasonMessage()
        {
            var shutdownReason = sharedShutdownReasonMessageRaw;
            if (string.IsNullOrEmpty(shutdownReason))
            {
                return string.Empty;
            }

            ExtractHeader(ref shutdownReason,
                          out var header);

            var message = header;
            if (message is not null)
            {
                message += "[br]" + shutdownReason;
            }
            else
            {
                message = shutdownReason;
            }

            return message;
        }

        private static uint SharedGetSecondsRemains()
        {
            var timeRemains = Math.Max(0,
                                       sharedServerShutdownServerTime - Client.CurrentGame.ServerFrameTimeRounded);
            var secondsRemains = (uint)Math.Round(timeRemains, MidpointRounding.AwayFromZero);
            return secondsRemains;
        }

        private static string SharedGetShutdownMessage()
        {
            var secondsRemains = SharedGetSecondsRemains();
            return string.Format(NotificationShutdown_MessageFormat,
                                 ClientTimeFormatHelper.FormatTimeDuration(secondsRemains),
                                 GetFormattedShutdownReasonMessage());
        }

        private static string SharedGetShutdownNotificationTitle()
        {
            var rawMessage = sharedShutdownReasonMessageRaw;
            ExtractHeader(ref rawMessage,
                          out var header);

            return string.IsNullOrEmpty(header)
                       ? NotificationShutdown_Title
                       : Message_ServerReboot;
        }

        private void ClientCurrentGameConnectionStateChangedHandler()
        {
            if (Client.CurrentGame.ConnectionState
                    is ConnectionState.Connecting
                    or ConnectionState.Connected)
            {
                sharedShutdownReasonMessageRaw = null;
                sharedServerShutdownServerTime = 0;
            }
        }

        private void ClientRemote_ShutdownNotification(
            string message,
            double serverTime)
        {
            this.clientNotification?.Hide(quick: true); // hide already existing notification

            sharedShutdownReasonMessageRaw = message;
            sharedServerShutdownServerTime = serverTime;

            Logger.Important(
                string.Format(NotificationShutdown_MessageFormat,
                              ClientTimeFormatHelper.FormatTimeDuration(SharedGetSecondsRemains()),
                              GetFormattedShutdownReasonMessage()));

            this.clientNotification = NotificationSystem.ClientShowNotification(
                title: SharedGetShutdownNotificationTitle(),
                message: SharedGetShutdownMessage(),
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

            var secondsRemains = SharedGetSecondsRemains();
            if (secondsRemains <= 0)
            {
                return;
            }

            this.clientNotification.Message = SharedGetShutdownMessage();
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
                _ => _.ClientRemote_ShutdownNotification(sharedShutdownReasonMessageRaw,
                                                         sharedServerShutdownServerTime));
        }

        private void ServerShutdownNotificationHandler(string message, double time)
        {
            sharedShutdownReasonMessageRaw = message;
            sharedServerShutdownServerTime = time;

            this.CallClient(
                Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                _ => _.ClientRemote_ShutdownNotification(sharedShutdownReasonMessageRaw,
                                                         sharedServerShutdownServerTime));
        }
    }
}