namespace AtomicTorch.CBND.CoreMod.Systems.RaidingProtection
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// It's a raiding window/hours system for PvP server. Server owner could configure hours when every day raiding is
    /// possible.
    /// </summary>
    public class RaidingProtectionSystem : ProtoSystem<RaidingProtectionSystem>
    {
        public const string Notification_CannotDamageUnderRaidingProtection_Message =
            "Damaging any structures inside the land claim is only possible during the raid window.";

        public const string Notification_CannotDamageUnderRaidingProtection_Title =
            "No damage";

        public const string Notification_RaidingWindowNowClosed_Message =
            @"Raiding window is now closed.
              [br]The ongoing raids may continue.";

        private const string DatabaseKeyRaidingWindowUTC = "DatabaseKeyRaidingWindowUTC";

        public static event Action ClientRaidingWindowChanged;

        public static bool ClientIsRaidingWindowEnabled => ClientRaidingWindowUTC.DurationHours < 24;

        public static DayTimeInterval ClientRaidingWindowUTC { get; private set; }
            = new(0, 24);

        public static bool ServerIsRaidingWindowEnabled => ServerRaidingWindowUTC.DurationHours < 24;

        public static DayTimeInterval ServerRaidingWindowUTC { get; private set; }
            = new(0, 24);

        public static bool SharedIsRaidingWindowNow
        {
            get
            {
                if (IsServer)
                {
                    if (!ServerIsRaidingWindowEnabled)
                    {
                        return true;
                    }
                }
                // if client
                else if (!ClientIsRaidingWindowEnabled)
                {
                    return true;
                }

                // either client or server has raiding window
                return SharedCalculateHoursUntilNextRaid() <= 0;
            }
        }

        public static void ClientShowNotificationRaidingNotAvailableIfNecessary()
        {
            if (SharedIsRaidingWindowNow)
            {
                return;
            }

            // check a small space near the player
            var characterPosition = ClientCurrentCharacterHelper.Character.TilePosition;
            var checkSize = 4;
            var checkBounds = new RectangleInt(x: characterPosition.X - checkSize / 2,
                                               y: characterPosition.Y - checkSize / 2,
                                               width: checkSize,
                                               height: checkSize);

            SharedCanRaid(checkBounds, showClientNotification: true);
        }

        // This notification should be displayed when raiding window is just closed.
        public static void ClientShowNotificationRaidingWindowNowClosed()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            NotificationSystem.ClientShowNotification(
                title: CoreStrings.WindowPolitics_RaidingRestriction_Title,
                message: Notification_RaidingWindowNowClosed_Message
                         + "[br]"
                         + Notification_CannotDamageUnderRaidingProtection_Message,
                onClick: Menu.Open<WindowPolitics>);
        }

        // This notification should be displayed when raiding window is just opened
        // or when player is logged in during the opened raiding window
        public static void ClientShowNotificationRaidingWindowNowOpened()
        {
            var timeUntilRaidEnds = SharedCalculateTimeUntilRaidEnds();
            if (timeUntilRaidEnds.TotalSeconds < 30)
            {
                return;
            }

            NotificationSystem.ClientShowNotification(
                CoreStrings.WindowPolitics_RaidingRestriction_Title,
                string.Format(CoreStrings.WindowPolitics_RaidingRestriction_CurrentRaidWindow,
                              ClientTimeFormatHelper.FormatTimeDuration(timeUntilRaidEnds, trimRemainder: true)),
                onClick: Menu.Open<WindowPolitics>);
        }

        public static void ServerNotifyShowNotificationRaidingNotAvailableNow(
            IReadOnlyCollection<ICharacter> characters)
        {
            Instance.CallClient(characters,
                                _ => _.ClientRemote_ShowNotificationRaidingNotAvailableNow());
        }

        public static void ServerSetRaidingWindow(DayTimeInterval newWindowUTC)
        {
            if (newWindowUTC.DurationHours <= 0
                || newWindowUTC.DurationHours > 24)
            {
                // full day raiding - disable raiding protection
                newWindowUTC = new DayTimeInterval(fromHour: newWindowUTC.FromHour,
                                                   toHour: newWindowUTC.FromHour + 24);
            }

            if (ServerRaidingWindowUTC.Equals(newWindowUTC))
            {
                return;
            }

            ServerRaidingWindowUTC = newWindowUTC;
            Server.Database.Set(nameof(RaidingProtectionSystem),
                                DatabaseKeyRaidingWindowUTC,
                                ServerRaidingWindowUTC);
            Logger.Important($"Set raiding window: {ServerRaidingWindowUTC} (UTC time)");

            var allCharacters = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true,
                                                                               exceptSpectators: false);

            Instance.CallClient(allCharacters,
                                _ => _.ClientRemote_RaidingWindowInfo(ServerRaidingWindowUTC));
        }

        public static TimeSpan SharedCalculateTimeUntilNextRaid()
        {
            var hoursRemainsToStart = SharedCalculateHoursUntilNextRaid();
            return TimeSpan.FromHours(hoursRemainsToStart);
        }

        public static TimeSpan SharedCalculateTimeUntilRaidEnds()
        {
            var hoursRemainsToRaidEnd = SharedCalculateHoursUntilRaidEnds();
            return TimeSpan.FromHours(hoursRemainsToRaidEnd);
        }

        /// <summary>
        /// Returns true when it's possible to apply structure damage in the provided bounds
        /// (when the raiding protection not applies).
        /// </summary>
        public static bool SharedCanRaid(
            RectangleInt targetObjectBounds,
            bool showClientNotification)
        {
            if (SharedIsRaidingWindowNow)
            {
                return true;
            }

            SharedFindLandClaimArea(targetObjectBounds,
                                    out _,
                                    out var isThereAnyLandClaimAreaUnderRaid);

            if (isThereAnyLandClaimAreaUnderRaid)
            {
                // always allow damage to structures in land claim areas under raid
                return true;
            }

            if (IsClient && showClientNotification)
            {
                ClientShowNotificationRaidingNotAvailableNow();
            }

            return false;
        }

        /// <summary>
        /// Returns true when it's possible to apply structure damage to a particular structure.
        /// </summary>
        public static bool SharedCanRaid(
            IStaticWorldObject targetObject,
            bool showClientNotification)
        {
            if (targetObject.ProtoWorldObject is not IProtoObjectStructure)
            {
                return true;
            }

            if (SharedIsRaidingWindowNow)
            {
                return true;
            }

            // the raiding window is not now so we need to perform some checks to ensure the damage is allowed
            var targetObjectBounds = targetObject.Bounds;
            SharedFindLandClaimArea(targetObjectBounds,
                                    out var isThereAnyLandClaimArea,
                                    out var isThereAnyLandClaimAreaUnderRaid);

            if (!isThereAnyLandClaimArea)
            {
                // always allow damage to structures outside of land claim areas
                return true;
            }

            if (isThereAnyLandClaimAreaUnderRaid)
            {
                // always allow damage to structures in land claim areas under raid
                return true;
            }

            // don't allow damage as the structure is inside the land claim area (which is not under raid)
            if (IsClient && showClientNotification)
            {
                ClientShowNotificationRaidingNotAvailableNow();
            }

            return false;
        }

        private static void ClientShowNotificationRaidingNotAvailableNow()
        {
            NotificationSystem.ClientShowNotification(
                Notification_CannotDamageUnderRaidingProtection_Title,
                Notification_CannotDamageUnderRaidingProtection_Message,
                onClick: Menu.Open<WindowPolitics>);
        }

        private static double SharedCalculateHoursUntilNextRaid()
        {
            var raidingWindow = IsServer
                                    ? ServerRaidingWindowUTC
                                    : ClientRaidingWindowUTC;

            var serverUtcHours = DateTime.Now.ToUniversalTime().TimeOfDay.TotalHours;
            var hoursRemainsToStart = raidingWindow.SharedCalculateHoursRemainsToStart(serverUtcHours);
            return hoursRemainsToStart;
        }

        private static double SharedCalculateHoursUntilRaidEnds()
        {
            var raidingWindow = IsServer
                                    ? ServerRaidingWindowUTC
                                    : ClientRaidingWindowUTC;

            var serverUtcHours = DateTime.Now.ToUniversalTime().TimeOfDay.TotalHours;
            var hoursRemainsToEnd = raidingWindow.SharedCalculateHoursRemainsToEnd(serverUtcHours);
            return hoursRemainsToEnd;
        }

        private static void SharedFindLandClaimArea(
            RectangleInt targetObjectBounds,
            out bool isThereAnyLandClaimArea,
            out bool isThereAnyLandClaimAreaUnderRaid)
        {
            isThereAnyLandClaimArea = false;
            isThereAnyLandClaimAreaUnderRaid = false;
            using var landClaimAreas = Api.Shared.GetTempList<ILogicObject>();
            LandClaimSystem.SharedGetAreasInBounds(targetObjectBounds,
                                                   landClaimAreas,
                                                   addGracePadding: false);
            foreach (var area in landClaimAreas.AsList())
            {
                isThereAnyLandClaimArea = true;
                if (!LandClaimSystem.SharedIsAreaUnderRaid(area))
                {
                    continue;
                }

                isThereAnyLandClaimAreaUnderRaid = true;
                return;
            }
        }

        private void ClientRemote_RaidingWindowInfo(DayTimeInterval gameDayTimeInterval)
        {
            Logger.Important($"Received raiding window: {gameDayTimeInterval} (UTC time)");

            ClientRaidingWindowUTC = gameDayTimeInterval;
            if (ClientRaidingWindowChanged is not null)
            {
                Api.SafeInvoke(ClientRaidingWindowChanged);
            }
        }

        private void ClientRemote_ShowNotificationRaidingNotAvailableNow()
        {
            ClientShowNotificationRaidingNotAvailableNow();
        }

        [RemoteCallSettings(timeInterval: RemoteCallSettingsAttribute.MaxTimeInterval)]
        private void ServerRemote_RequestRaidingWindowInfo()
        {
            this.CallClient(ServerRemoteContext.Character,
                            _ => _.ClientRemote_RaidingWindowInfo(ServerRaidingWindowUTC));
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;

                Refresh();
                ClientRaidingWindowNotificationManager.Initialize();

                void Refresh()
                {
                    if (Api.Client.Characters.CurrentPlayerCharacter is not null)
                    {
                        Instance.CallServer(_ => _.ServerRemote_RequestRaidingWindowInfo());
                    }
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                if (Server.Database.TryGet(nameof(RaidingProtectionSystem),
                                           DatabaseKeyRaidingWindowUTC,
                                           out DayTimeInterval window))
                {
                    ServerRaidingWindowUTC = window;
                    Logger.Important($"Loaded raiding window: {ServerRaidingWindowUTC} (UTC time)");
                }
                else
                {
                    // no raiding window - unrestricted raiding hours
                    ServerRaidingWindowUTC = new DayTimeInterval(0, 24);
                }
            }
        }
    }
}