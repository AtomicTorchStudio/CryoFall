namespace AtomicTorch.CBND.CoreMod.Systems.ServerOperator
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ServerOperatorSystem : ProtoSystem<ServerOperatorSystem>
    {
        private const string DatabaseEntryId = "ServerOperatorCharactersIdsList";

        private static bool clientIsServerOperator;

        private static HashSet<string> serverOperatorsList;

        public static event Action ClientIsOperatorChanged;

        public static IReadOnlyCollection<string> ServerOperatorsList
        {
            get
            {
                Api.ValidateIsServer();
                return serverOperatorsList;
            }
        }

        public override string Name => "Server operator system";

        public static bool ClientIsOperator()
        {
            return SharedIsOperator(
                Client.Characters.CurrentPlayerCharacter);
        }

        public static void ClientRequestCurrentUserIsOperator()
        {
            Instance.CallServer(_ => _.ServerRemote_RequestCurrentUserIsOperator());
        }

        public static void ServerAdd(ICharacter character)
        {
            if (character.IsNpc)
            {
                throw new Exception("NPC cannot be a server operator");
            }

            ServerAdd(character.Name);
        }

        public static void ServerAdd(string name)
        {
            ServerSetOperatorAccess(name, isOperator: true);
        }

        public static bool ServerIsOperator(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }

            return serverOperatorsList.Contains(username);
        }

        public static void ServerRemove(ICharacter character)
        {
            if (character.IsNpc)
            {
                throw new Exception("NPC cannot be a server operator");
            }

            ServerRemove(character.Name);
        }

        public static void ServerRemove(string name)
        {
            ServerSetOperatorAccess(name, isOperator: false);
        }

        public static void Setup(IServerConfiguration serverConfiguration)
        {
            serverConfiguration.SetupPlayerCheckServerOperatorAccessHook(
                ServerIsOperator);
        }

        public static bool SharedIsOperator(ICharacter character)
        {
            if (character == null)
            {
                return false;
            }

            if (IsServer)
            {
                return serverOperatorsList.Contains(character.Name);
            }

            return clientIsServerOperator
                   && Client.Characters.CurrentPlayerCharacter == character;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            // below is the Server-side code only
            if (Api.Server.Database.TryGet(
                nameof(ServerOperatorSystem),
                DatabaseEntryId,
                out serverOperatorsList))
            {
                ServerLogOperatorsList();
                return;
            }

            // operators list is not stored, create a new one
            serverOperatorsList = new HashSet<string>(StringComparer.Ordinal)
            {
                // we're great developers, promise not to hack your server!
                // just joking, we need this for our servers otherwise it will be much harder to manage them
                "ai_enabled",
                "Lurler"
            };

            Api.Server.Database.Set(
                nameof(ServerOperatorSystem),
                DatabaseEntryId,
                serverOperatorsList);

            ServerLogOperatorsList();
        }

        private static void ServerLogOperatorsList()
        {
            Logger.Important(
                "Server operators system - server operators list: "
                + serverOperatorsList.GetJoinedString());
        }

        private static void ServerSetOperatorAccess(string name, bool isOperator)
        {
            var isChanged = isOperator
                                ? serverOperatorsList.Add(name)
                                : serverOperatorsList.Remove(name);

            if (!isChanged)
            {
                return;
            }

            Logger.Warning(
                $"Server operator mode is changed: \"{name}\" - operator mode is {(isOperator ? "enabled" : "disabled")}");

            ServerLogOperatorsList();

            var character = Server.Characters.GetPlayerCharacter(name);
            if (character == null)
            {
                return;
            }

            // notify that player
            Instance.CallClient(
                character,
                _ => _.ClientRemote_SetCurrentUserIsOperator(isOperator));
        }

        private void ClientRemote_SetCurrentUserIsOperator(bool isOperator)
        {
            clientIsServerOperator = isOperator;
            Logger.Important("Received isOperator=" + isOperator);
            if (ClientIsOperatorChanged != null)
            {
                Api.SafeInvoke(ClientIsOperatorChanged);
            }
        }

        private void ServerRemote_RequestCurrentUserIsOperator()
        {
            var character = ServerRemoteContext.Character;
            if (Api.IsEditor)
            {
                // automatically enable editor mode server operator for current payer in Editor
                // (and do this only once)
                if (!Api.Server.Database.TryGet(nameof(ServerOperatorSystem),
                                                "EditorOperatorFirstTimeSet",
                                                out bool _))
                {
                    Api.Server.Database.Set(nameof(ServerOperatorSystem),
                                            "EditorOperatorFirstTimeSet",
                                            true);
                    ServerAdd(character);
                }
            }

            var isOperator = SharedIsOperator(character);
            this.CallClient(character, _ => _.ClientRemote_SetCurrentUserIsOperator(isOperator));
        }
    }
}