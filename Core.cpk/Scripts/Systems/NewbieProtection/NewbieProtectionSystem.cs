namespace AtomicTorch.CBND.CoreMod.Systems.NewbieProtection
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class NewbieProtectionSystem : ProtoSystem<NewbieProtectionSystem>
    {
        public const string Button_CancelNewbieProtection =
            "Cancel newbie protection";

        public const string Dialog_CancelNewbieProtection =
            "Are you sure you want to cancel the newbie protection? You cannot enable it again.";

        public const string NewbieProtectionDescription =
            "You're under newbie protection. It protects you against losing your items and learning points if your character is killed by another player. Also, the items you drop in other cases of death cannot be looted by other players. Please follow the quests to craft the necessary tools and weapons, and build a small base as soon as possible!";

        /// <summary>
        /// Duration of newbie protection (in seconds).
        /// </summary>
        public const double NewbieProtectionDuration = 4 * 60 * 60; // 4 hours

        public const string NewbieProtectionExpireInFormat =
            "The newbie protection will expire in:";

        public const string Notification_CannotDamageOtherPlayersOrLootBags =
            "While under newbie protection you cannot attack other players or loot their bags or items. But you can cancel this protection at any time in the politics menu.";

        public const string Notification_CannotPerformActionWhileUnderProtection =
            "You cannot perform this action while under newbie protection. But you can cancel this protection at any time in the politics menu.";

        public const string Notification_LootBagUnderProtection =
            "This bag is under newbie protection, so only the owner can pick it up.";

        public const string Notification_ProtectionExpired =
            "The newbie protection has expired. You're now free to engage in PvP fights, but beware—your character will drop all items upon death.";

        public const string Title_NewbieProtection =
            "Newbie protection";

        private const string DatabaseNewbieLastDeathIsPvPlistEntryId = "ServerNewbieLastDeathIsPvPlist";

        private const string DatabaseNewbiesListEntryId = "ServerNewbiesList";

        private const int ServerUpdateInterval = 10;

        private static HashSet<ICharacter> serverNewbieLastDeathIsPvPlist;

        private static List<(ICharacter character, double timeRemains)> serverNewbies;

        public static event Action<double> ClientNewbieProtectionTimeRemainingReceived;

        public static bool ClientIsNewbie => SharedIsNewbie(Client.Characters.CurrentPlayerCharacter);

        public static double ClientNewbieProtectionTimeRemaining { get; private set; }

        [NotLocalizable]
        public override string Name => "Newbie protection system";

        public static void ClientDisableNewbieProtection()
        {
            Instance.CallServer(_ => _.ServerRemote_ServerDisableNewbieProtection());
        }

        public static Task<bool> ClientGetLatestDeathIsNewbiePvP()
        {
            return Instance.CallServer(_ => _.ServerRemote_ServerGetLatestDeathIsNewbiePvP());
        }

        public static void ClientShowNewbieCannotDamageOtherPlayersOrLootBags(bool isLootBag)
        {
            NotificationSystem.ClientShowNotification(
                title: Notification_CannotDamageOtherPlayersOrLootBags,
                icon: isLootBag
                          ? Api.GetProtoEntity<ObjectPlayerLootContainer>().Icon
                          : null,
                onClick: Menu.Open<WindowSocial>);
        }

        public static void ServerDisableNewbieProtection(ICharacter character)
        {
            for (var index = 0; index < serverNewbies.Count; index++)
            {
                var tuple = serverNewbies[index];
                if (tuple.character != character)
                {
                    continue;
                }

                serverNewbies.RemoveAt(index--);
                PlayerCharacter.GetPublicState(character).IsNewbie = false;
                Logger.Info("Newbie protection removed", character);

                ServerSendNewbieProtectionTimeRemaining(character);
            }
        }

        /// <summary>
        /// Gets the remaining duration of the newbie protection.
        /// </summary>
        public static double ServerGetNewbieProtectionTimeRemaining(ICharacter character)
        {
            if (!PlayerCharacter.GetPublicState(character).IsNewbie)
            {
                return 0;
            }

            foreach (var tuple in serverNewbies)
            {
                if (tuple.character == character)
                {
                    return tuple.timeRemains;
                }
            }

            // should never happen
            Logger.Error("Cannot find newbie entry to get the protection remaining duration", character);
            return 0;
        }

        public static void ServerRegisterDeath(
            ICharacter character,
            bool isPvPdeath,
            out bool shouldDropLootAndLoseLP)
        {
            if (isPvPdeath && SharedIsNewbie(character))
            {
                // newbie's PvP death
                shouldDropLootAndLoseLP = false;
                serverNewbieLastDeathIsPvPlist.Add(character);
            }
            else
            {
                // not a newbie or newbie's PvE death
                shouldDropLootAndLoseLP = true;
                serverNewbieLastDeathIsPvPlist.Remove(character);
            }
        }

        public static void ServerRegisterNewbie(ICharacter character)
        {
            if (PveSystem.ServerIsPvE)
            {
                // no newbie protection on PvE servers as it's not required
                return;
            }

            Api.Assert(character != null && !character.IsNpc,
                       "Please provide a player character instance");

            var isFound = false;
            for (var index = 0; index < serverNewbies.Count; index++)
            {
                var tuple = serverNewbies[index];
                if (tuple.character != character)
                {
                    continue;
                }

                isFound = true;
                serverNewbies[index] = (character, timeRemains: NewbieProtectionDuration);
            }

            if (!isFound)
            {
                serverNewbies.Add((character, timeRemains: NewbieProtectionDuration));
            }

            PlayerCharacter.GetPublicState(character)
                           .IsNewbie = true;

            Logger.Info("Newbie registered", character);
            ServerSendNewbieProtectionTimeRemaining(character);
        }

        public static bool SharedIsNewbie(ICharacter character)
        {
            if (character == null
                || character.IsNpc)
            {
                return false;
            }

            return PlayerCharacter.GetPublicState(character)
                                  .IsNewbie;
        }

        public static void SharedNotifyNewbieCannotPerformAction(
            ICharacter character,
            IProtoGameObject iconSource)
        {
            if (IsClient)
            {
                ClientNotifyNewbieCannotPerformAction(iconSource);
                return;
            }

            Instance.CallClient(character,
                                _ => _.ClientRemote_NotifyNewbieCannotPerformAction(iconSource));
        }

        public static void SharedShowNewbieCannotDamageOtherPlayersOrLootBags(ICharacter character, bool isLootBag)
        {
            if (IsClient)
            {
                ClientShowNewbieCannotDamageOtherPlayersOrLootBags(isLootBag);
            }
            else
            {
                Instance.CallClient(character,
                                    _ => _.ClientRemote_ClientShowNewbieCannotPickupLootNotification(isLootBag));
            }
        }

        public static void SharedValidateNewbieCannotPickupItemsDuringRaidAnotherArea(
            ICharacter character,
            Vector2Ushort position,
            out bool isAllowedToPickup,
            bool writeToLog)
        {
            if (!SharedIsNewbie(character)
                || CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                isAllowedToPickup = true;
                return;
            }

            foreach (var area in LandClaimSystem.SharedEnumerateAllAreas())
            {
                var areaBounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area, addGracePadding: true);
                if (!areaBounds.Contains(position))
                {
                    continue;
                }

                // Please note: this check will not work on client for other players areas
                // as player doesn't have their private state info.
                // Anyway, it's not a big deal as the check could be done on the server side.
                if (LandClaimSystem.SharedIsAreaUnderRaid(area)
                    && !LandClaimSystem.SharedIsOwnedArea(area, character))
                {
                    // cannot pickup - there is an area under raid and current player is not the area owner
                    isAllowedToPickup = false;

                    if (writeToLog)
                    {
                        SharedNotifyNewbieCannotPerformAction(character, iconSource: null);
                    }

                    return;
                }
            }

            isAllowedToPickup = true;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            // below is the Server-side code only
            if (!Api.Server.Database.TryGet(
                    nameof(NewbieProtectionSystem),
                    DatabaseNewbiesListEntryId,
                    out serverNewbies))
            {
                // newbies list is not stored, create a new one
                serverNewbies = new List<(ICharacter character, double timeRemains)>();
                Api.Server.Database.Set(
                    nameof(NewbieProtectionSystem),
                    DatabaseNewbiesListEntryId,
                    serverNewbies);
            }
            else
            {
                // process loaded list of newbies
                for (var index = 0; index < serverNewbies.Count; index++)
                {
                    var tuple = serverNewbies[index];
                    var character = tuple.character;
                    if (character == null)
                    {
                        serverNewbies.RemoveAt(index--);
                        continue;
                    }

                    var publicState = PlayerCharacter.GetPublicState(character);
                    publicState.IsNewbie = true;
                }
            }

            if (!Api.Server.Database.TryGet(
                    nameof(NewbieProtectionSystem),
                    DatabaseNewbieLastDeathIsPvPlistEntryId,
                    out serverNewbieLastDeathIsPvPlist))
            {
                // newbies last death list is not stored, create a new one
                serverNewbieLastDeathIsPvPlist = new HashSet<ICharacter>();
                Api.Server.Database.Set(
                    nameof(NewbieProtectionSystem),
                    DatabaseNewbieLastDeathIsPvPlistEntryId,
                    serverNewbieLastDeathIsPvPlist);
            }

            TriggerTimeInterval.ServerConfigureAndRegister(
                callback: this.ServerUpdate,
                name: "System." + this.ShortId,
                interval: TimeSpan.FromSeconds(ServerUpdateInterval));
        }

        private static void ClientNotifyNewbieCannotPerformAction(IProtoGameObject iconSource)
        {
            ITextureResource icon;
            switch (iconSource)
            {
                case IProtoStaticWorldObject protoStaticWorld:
                    icon = protoStaticWorld.Icon;
                    break;

                case IProtoItem protoItem:
                    icon = protoItem.Icon;
                    break;

                default:
                    icon = null;
                    break;
            }

            NotificationSystem.ClientShowNotification(
                title: Notification_CannotPerformActionWhileUnderProtection,
                icon: icon,
                onClick: Menu.Open<WindowSocial>);
        }

        private static void ServerSendNewbieProtectionTimeRemaining(ICharacter character)
        {
            if (!character.IsOnline)
            {
                return;
            }

            var timeRemaining = ServerGetNewbieProtectionTimeRemaining(character);
            Instance.CallClient(character, _ => _.ClientRemote_SetNewbieProtectionDuration(timeRemaining));
        }

        private void ClientRemote_ClientShowNewbieCannotPickupLootNotification(bool isLootBag)
        {
            ClientShowNewbieCannotDamageOtherPlayersOrLootBags(isLootBag);
        }

        private void ClientRemote_NotifyNewbieCannotPerformAction(IProtoGameObject iconSource)
        {
            ClientNotifyNewbieCannotPerformAction(iconSource);
        }

        private void ClientRemote_SetNewbieProtectionDuration(double timeRemaining)
        {
            Logger.Info("Received newbie protection time remaining: "
                        + ClientTimeFormatHelper.FormatTimeDuration(timeRemaining));

            if (ClientNewbieProtectionTimeRemaining > timeRemaining
                && timeRemaining <= 0)
            {
                NotificationSystem.ClientShowNotification(
                    title: Notification_ProtectionExpired,
                    autoHide: false);
            }

            ClientNewbieProtectionTimeRemaining = timeRemaining;
            Api.SafeInvoke(
                () => ClientNewbieProtectionTimeRemainingReceived?.Invoke(timeRemaining));
        }

        private void ServerRemote_GetNewbieProtectionTimeRemaining()
        {
            ServerSendNewbieProtectionTimeRemaining(ServerRemoteContext.Character);
        }

        private void ServerRemote_ServerDisableNewbieProtection()
        {
            ServerDisableNewbieProtection(ServerRemoteContext.Character);
        }

        private bool ServerRemote_ServerGetLatestDeathIsNewbiePvP()
        {
            var character = ServerRemoteContext.Character;
            return serverNewbieLastDeathIsPvPlist.Contains(character);
        }

        private void ServerUpdate()
        {
            for (var index = 0; index < serverNewbies.Count; index++)
            {
                var tuple = serverNewbies[index];
                var character = tuple.character;
                if (!character.IsOnline)
                {
                    // time deducted only for the online characters
                    continue;
                }

                tuple.timeRemains -= ServerUpdateInterval;
                if (tuple.timeRemains > 0)
                {
                    serverNewbies[index] = tuple;
                    continue;
                }

                // newbie protection expired
                serverNewbies.RemoveAt(index--);
                PlayerCharacter.GetPublicState(character).IsNewbie = false;
                Logger.Info("Newbie protection expired", character);
                ServerSendNewbieProtectionTimeRemaining(character);
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;

                Refresh();

                void Refresh()
                {
                    if (Api.Client.Characters.CurrentPlayerCharacter == null)
                    {
                        ClientNewbieProtectionTimeRemaining = 0;
                        Api.SafeInvoke(
                            () => ClientNewbieProtectionTimeRemainingReceived?.Invoke(0));
                        return;
                    }

                    Instance.CallServer(_ => _.ServerRemote_GetNewbieProtectionTimeRemaining());
                }
            }
        }
    }
}