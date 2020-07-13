namespace AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.ServerModerator;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;

    public static class ServerPlayerAccessSystem
    {
        public const string CannotJoinBanned =
            "You have been banned from this server by the server operator";

        public const string CannotJoinKicked =
            @"You've been kicked from this server temporarily. Please consult server operators regarding any additional details.
              [br][br]You can rejoin the server in {0}.";

        public const string CannotJoinNotInWhitelist =
            @"Operator of this server has set it to invites only.
              [br]Please check server description for any possible additional details.";

        private const string DatabaseKeyIsWhitelistEnabled = "IsWhitelistEnabled";

        private const string DatabaseKeyKickedTillDictionary = "KickedTillDictionary";

        private static readonly IDatabaseService Database = Api.Server.Database;

        private static List<string> blackList;

        private static bool isWhiteListEnabled;

        private static Dictionary<string, double> kickedTillDictionary;

        private static List<string> whiteList;

        /// <summary>
        /// Please use only for logging purposes.
        /// </summary>
        public static IEnumerable<string> GetBlackList()
        {
            return blackList.OrderBy(n => n);
        }

        /// <summary>
        /// Please use only for logging purposes.
        /// </summary>
        public static IEnumerable<string> GetKickList()
        {
            return kickedTillDictionary.Keys.OrderBy(n => n);
        }

        /// <summary>
        /// Please use only for logging purposes.
        /// </summary>
        public static IEnumerable<string> GetWhiteList()
        {
            return whiteList.OrderBy(n => n);
        }

        public static bool IsInBlackList(string playerName)
        {
            return blackList.Contains(playerName, StringComparer.OrdinalIgnoreCase);
        }

        public static void Kick(ICharacter character, int minutes)
        {
            if (character.IsNpc)
            {
                throw new Exception("Cannot kick NPC character");
            }

            kickedTillDictionary[character.Name] = Api.Server.Game.FrameTime + 60 * minutes;
            Api.Logger.Important($"Player kicked: {character}. Kick duration: {minutes} minutes");
            RefreshCurrentCharacters();
        }

        public static bool SetBlackListEntry(string playerName, bool isEnabled)
        {
            return ModifyAccessList(isWhiteList: false, playerName, isEnabled);
        }

        public static bool SetWhiteListEntry(string playerName, bool isEnabled)
        {
            return ModifyAccessList(isWhiteList: true, playerName, isEnabled);
        }

        public static void SetWhitelistModeEnabled(bool isEnabled)
        {
            if (isWhiteListEnabled == isEnabled)
            {
                return;
            }

            isWhiteListEnabled = isEnabled;
            Database.Set(nameof(ServerPlayerAccessSystem),
                         DatabaseKeyIsWhitelistEnabled,
                         isWhiteListEnabled);

            Api.Logger.Important("Whitelist mode changed: " + (isWhiteListEnabled ? "enabled" : "disabled"));

            RefreshCurrentCharacters();
        }

        public static bool Unkick(ICharacter character)
        {
            if (!kickedTillDictionary.Remove(character.Name))
            {
                return false;
            }

            Api.Logger.Important("Player unkicked: " + character);
            return true;
        }

        private static bool IsKicked(string playerName, out double secondsRemains)
        {
            if (!kickedTillDictionary.TryGetValue(playerName, out var kickedTillTime))
            {
                // not kicked
                secondsRemains = 0;
                return false;
            }

            secondsRemains = kickedTillTime - Api.Server.Game.FrameTime;
            return secondsRemains > 1;
        }

        private static bool ModifyAccessList(bool isWhiteList, string playerName, bool isEnabled)
        {
            var list = isWhiteList ? whiteList : blackList;
            var contains = list.Contains(playerName, StringComparer.OrdinalIgnoreCase);
            if (!contains
                && !isEnabled)
            {
                return false;
            }

            if (isEnabled)
            {
                if (!contains)
                {
                    list.Add(playerName);
                    Api.Logger.Important(
                        $"Player {playerName} added to the {(isWhiteList ? "whitelist" : "blacklist")}");
                    RefreshCurrentCharacters();
                    return true;
                }

                return false;
            }

            if (list.Remove(playerName))
            {
                Api.Logger.Important(
                    $"Player {playerName} removed from the {(isWhiteList ? "whitelist" : "blacklist")}");
                RefreshCurrentCharacters();
                return true;
            }

            return false;
        }

        private static void PlayerLoginHook(string playerName, out string errorMessage)
        {
            if (ServerOperatorSystem.ServerIsOperator(playerName)
                || ServerModeratorSystem.ServerIsModerator(playerName))
            {
                // operators/moderators cannot be blocked or kicked
                errorMessage = null;
                return;
            }

            if (IsInBlackList(playerName))
            {
                errorMessage = CannotJoinBanned;
                return;
            }

            if (isWhiteListEnabled && !whiteList.Contains(playerName, StringComparer.OrdinalIgnoreCase))
            {
                errorMessage = CannotJoinNotInWhitelist;
                return;
            }

            if (IsKicked(playerName, out var secondsRemains))
            {
                errorMessage = string.Format(CannotJoinKicked,
                                             ClientTimeFormatHelper.FormatTimeDuration(secondsRemains));
                return;
            }

            // all checks passed successfully
            errorMessage = null;
        }

        private static void RefreshCurrentCharacters()
        {
            // process all the connected players and drop such of them that should not have access
            var onlinePlayerCharacters = Api.Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
            using var tempList = Api.Shared.WrapInTempList(onlinePlayerCharacters);
            foreach (var character in tempList.AsList())
            {
                PlayerLoginHook(character.Name, out var errorMessage);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    continue;
                }

                Api.Server.Core.DropConnection(character, errorMessage);
            }
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private static void ServerLogCharactersList(bool isWhiteList)
        {
            var list = isWhiteList ? whiteList : blackList;
            var sb = new StringBuilder("Server ")
                     .Append(isWhiteList ? "whitelist" : "blacklist")
                     .Append(" characters list: ");

            if (list.Count > 0)
            {
                sb.AppendLine()
                  .Append(list.GetJoinedString(separator: Environment.NewLine));
            }
            else
            {
                sb.Append("<empty list>");
            }

            Api.Logger.Important(sb);
        }

        private class Bootstrapper : BaseBootstrapper
        {
            [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                serverConfiguration.SetupPlayerLoginHook(PlayerLoginHook);

                Database.TryGet(nameof(ServerPlayerAccessSystem),
                                DatabaseKeyIsWhitelistEnabled,
                                out isWhiteListEnabled);

                if (!Database.TryGet(nameof(ServerPlayerAccessSystem),
                                     DatabaseKeyKickedTillDictionary,
                                     out kickedTillDictionary))
                {
                    kickedTillDictionary = new Dictionary<string, double>();
                    Database.Set(nameof(ServerPlayerAccessSystem),
                                 DatabaseKeyKickedTillDictionary,
                                 kickedTillDictionary);
                }

                whiteList = LoadOrCreateList(isWhiteList: true);
                ServerLogCharactersList(isWhiteList: true);

                blackList = LoadOrCreateList(isWhiteList: false);
                ServerLogCharactersList(isWhiteList: false);

                List<string> LoadOrCreateList(bool isWhiteList)
                {
                    var key = isWhiteList ? "WhiteList" : "BlackList";
                    if (Database.TryGet(nameof(ServerPlayerAccessSystem), key, out List<string> list)
                        && list != null)
                    {
                        return list;
                    }

                    list = new List<string>();
                    Database.Set(nameof(ServerPlayerAccessSystem), key, list);

                    return list;
                }
            }
        }
    }
}