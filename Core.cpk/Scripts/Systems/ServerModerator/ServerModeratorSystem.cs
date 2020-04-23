namespace AtomicTorch.CBND.CoreMod.Systems.ServerModerator
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public class ServerModeratorSystem : ProtoSystem<ServerModeratorSystem>
    {
        private static readonly ICoreServerModeratorsService ServerModerators
            = IsServer ? Server.Core.Moderators : null;

        private static bool clientIsServerModerator;

        public static event Action ClientIsModeratorChanged;

        public static IReadOnlyCollection<string> ServerModeratorsList
            => ServerModerators.Moderators;

        public override string Name => "Server moderator system";

        public static bool ClientIsModerator()
        {
            return SharedIsModerator(
                Client.Characters.CurrentPlayerCharacter);
        }

        public static void ClientRequestCurrentUserIsModerator()
        {
            Instance.CallServer(_ => _.ServerRemote_RequestCurrentUserIsModerator());
        }

        public static void ServerAdd(ICharacter character)
        {
            if (character.IsNpc)
            {
                throw new Exception("NPC cannot be a server moderator");
            }

            ServerAdd(character.Name);
        }

        public static void ServerAdd(string name)
        {
            ServerSetModeratorAccess(name, isModerator: true);
        }

        public static bool ServerIsModerator(string username)
        {
            return ServerModerators.IsModerator(username);
        }

        public static void ServerRemove(ICharacter character)
        {
            if (character.IsNpc)
            {
                throw new Exception("NPC cannot be a server moderator");
            }

            ServerRemove(character.Name);
        }

        public static void ServerRemove(string name)
        {
            ServerSetModeratorAccess(name, isModerator: false);
        }

        public static bool SharedIsModerator(ICharacter character)
        {
            if (ServerOperatorSystem.SharedIsOperator(character))
            {
                // all operators are moderators
                return true;
            }

            if (character is null)
            {
                return false;
            }

            if (IsServer)
            {
                return ServerModerators.IsModerator(character.Name);
            }

            return clientIsServerModerator
                   && character == Client.Characters.CurrentPlayerCharacter;
        }

        private static void ServerSetModeratorAccess(string name, bool isModerator)
        {
            if (Api.IsEditor)
            {
                isModerator = true;
            }

            var isChanged = ServerModerators.SetModeratorAccess(name, isModerator);
            if (!isChanged)
            {
                return;
            }

            Logger.Warning(
                $"Server moderator mode is changed: \"{name}\" - moderator mode is {(isModerator ? "enabled" : "disabled")}");

            var character = Server.Characters.GetPlayerCharacter(name);
            if (character is null)
            {
                return;
            }

            // notify that player
            Instance.CallClient(character,
                                _ => _.ClientRemote_SetCurrentUserIsModerator(isModerator));
        }

        private void ClientRemote_SetCurrentUserIsModerator(bool isModerator)
        {
            clientIsServerModerator = isModerator;
            Logger.Important(
                $"Server moderator status received: current player {(isModerator ? "is" : "is not")} a server moderator");
            if (ClientIsModeratorChanged != null)
            {
                Api.SafeInvoke(ClientIsModeratorChanged);
            }
        }

        private void ServerRemote_RequestCurrentUserIsModerator()
        {
            var character = ServerRemoteContext.Character;
            if (Api.IsEditor)
            {
                ServerAdd(character);
            }

            var isModerator = SharedIsModerator(character);
            this.CallClient(character,
                            _ => _.ClientRemote_SetCurrentUserIsModerator(isModerator));
        }
    }
}