namespace AtomicTorch.CBND.CoreMod.Systems.ServerOperator
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public class ServerOperatorSystem : ProtoSystem<ServerOperatorSystem>
    {
        private static readonly ICoreServerOperatorsService ServerOperators
            = IsServer ? Server.Core.Operators : null;

        private static bool clientIsServerOperator;

        public static event Action ClientIsOperatorChanged;

        public static IReadOnlyCollection<string> ServerOperatorsList
            => ServerOperators.Operators;

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
            return ServerOperators.IsOperator(username);
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
            serverConfiguration.SetupPlayerCheckServerOperatorAccessHook(ServerIsOperator);
        }

        public static bool SharedIsOperator(ICharacter character)
        {
            if (character is null)
            {
                return false;
            }

            if (IsServer)
            {
                return ServerOperators.IsOperator(character.Name);
            }

            return clientIsServerOperator
                   && character == Client.Characters.CurrentPlayerCharacter;
        }

        private static void ServerSetOperatorAccess(string name, bool isOperator)
        {
            if (Api.IsEditor)
            {
                isOperator = true;
            }

            var isChanged = ServerOperators.SetOperatorAccess(name, isOperator);
            if (!isChanged)
            {
                return;
            }

            Logger.Warning(
                $"Server operator mode is changed: \"{name}\" - operator mode is {(isOperator ? "enabled" : "disabled")}");

            var character = Server.Characters.GetPlayerCharacter(name);
            if (character is null)
            {
                return;
            }

            // notify that player
            Instance.CallClient(character,
                                _ => _.ClientRemote_SetCurrentUserIsOperator(isOperator));
        }

        private void ClientRemote_SetCurrentUserIsOperator(bool isOperator)
        {
            clientIsServerOperator = isOperator;
            Logger.Important(
                $"Server operator status received: current player {(isOperator ? "is" : "is not")} a server operator");
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
                ServerAdd(character);
            }

            var isOperator = SharedIsOperator(character);
            this.CallClient(character,
                            _ => _.ClientRemote_SetCurrentUserIsOperator(isOperator));
        }
    }
}