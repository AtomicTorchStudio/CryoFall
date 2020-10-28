namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.OnlinePlayers;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.ProfanityFiltering;
    using AtomicTorch.CBND.CoreMod.Systems.ServerModerator;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using JetBrains.Annotations;

    public class ChatSystem : ProtoSystem<ChatSystem>
    {
        public const string ErrorSlowDown_Format =
            "You are sending too many messages. Slow down a bit. You can send messages again after {0}.";

        public const int MaxChatEntryLength = 300;

        public const string NotificationCurrentPlayerMuted =
            "You are banned from participating in chat by server administrator. Your ban expires in: {0}";

        public const string PlayerWentOfflineChatMessageFormat = "{0} left the game";

        public const string PlayerWentOnlineChatMessageFormat = "{0} joined the game";

        private static readonly bool ServerIsTradeChatRoomEnabled;

        private static readonly Dictionary<ICharacter, List<ILogicObject>> ServerPrivateChatRoomsCache
            = new Dictionary<ICharacter, List<ILogicObject>>();

        private static ILogicObject sharedGlobalChatRoomHolder;

        private static ILogicObject sharedLocalChatRoomHolder;

        private static ILogicObject sharedTradeChatRoomHolder;

        static ChatSystem()
        {
            ServerIsTradeChatRoomEnabled
                = ServerRates.Get("IsTradeChatRoomEnabled",
                                  defaultValue: 1,
                                  description: @"Is ""Trade"" chat room available on this server?
                                                 You can set it to 0 to disable it")
                  > 0;
        }

        public delegate void ClientReceivedMessageDelegate(BaseChatRoom chatRoom, in ChatEntry chatEntry);

        public static event Action<BaseChatRoom> ClientChatRoomAdded;

        public static event ClientReceivedMessageDelegate ClientChatRoomMessageReceived;

        public static event Action<BaseChatRoom> ClientChatRoomRemoved;

        public override string Name => "Chat system";

        public static void ClientClosePrivateChat(ChatRoomPrivate privateChatRoom)
        {
            privateChatRoom.ClientSetOpenedOrClosedForCurrentCharacter(isClosed: true);

            var chatRoomHolder = (ILogicObject)privateChatRoom.GameObject;
            Instance.CallServer(_ => _.ServerRemote_ClientClosePrivateChat(chatRoomHolder));
        }

        public static void ClientOnChatRoomAdded(ILogicObject chatRoomHolder)
        {
            var chatRoom = SharedGetChatRoom(chatRoomHolder);
            switch (chatRoom)
            {
                case ChatRoomTrade _:
                    sharedTradeChatRoomHolder = chatRoomHolder;
                    break;

                case ChatRoomGlobal _:
                    sharedGlobalChatRoomHolder = chatRoomHolder;
                    break;

                case ChatRoomLocal _:
                    sharedLocalChatRoomHolder = chatRoomHolder;
                    break;
            }

            Logger.Info($"{nameof(ChatSystem)}: added a chat room - {chatRoom}");
            Api.SafeInvoke(() => ClientChatRoomAdded?.Invoke(chatRoom));
        }

        public static void ClientOnChatRoomRemoved(ILogicObject chatRoomHolder)
        {
            var chatRoom = SharedGetChatRoom(chatRoomHolder);
            Logger.Info($"{nameof(ChatSystem)}: removed a chat room - {chatRoom}");
            Api.SafeInvoke(() => ClientChatRoomRemoved?.Invoke(chatRoom));
        }

        public static async void ClientOpenPrivateChat(string withCharacterName)
        {
            var privateChat = await GetOrCreatePrivateChatAsync();
            if (privateChat is not null)
            {
                if (OpenChat(privateChat))
                {
                    return;
                }

                // chat not initialized yet - open on the next frame
                ClientTimersSystem.AddAction(delaySeconds: 0,
                                             () => OpenChat(privateChat));
            }

            bool OpenChat(ILogicObject logicObject)
            {
                if (!logicObject.IsInitialized)
                {
                    return false;
                }

                Menu.CloseAll();
                ChatPanel.Instance.OpenChat(SharedGetChatRoom(logicObject));
                return true;
            }

            Task<ILogicObject> GetOrCreatePrivateChatAsync()
            {
                var allChats = Client.World.GetGameObjectsOfProto<ILogicObject, ChatRoomHolder>();
                foreach (var chatRoomHolder in allChats)
                {
                    if (SharedGetChatRoom(chatRoomHolder) is ChatRoomPrivate privateChatRoom
                        && (privateChatRoom.CharacterA == withCharacterName
                            || privateChatRoom.CharacterB == withCharacterName))
                    {
                        // found a local chat instance with this character
                        privateChatRoom.ClientSetOpenedOrClosedForCurrentCharacter(isClosed: false);
                        return Task.FromResult(chatRoomHolder);
                    }
                }

                return Instance.CallServer(_ => _.ServerRemote_CreatePrivateChatRoom(withCharacterName));
            }
        }

        public static void ClientSendMessageToRoom(
            [NotNull] BaseChatRoom chatRoom,
            string message)
        {
            Logger.Important("Chat message sent: " + chatRoom + ": " + message);

            chatRoom.ClientOnMessageReceived(
                new ChatEntry(
                    ClientCurrentCharacterHelper.Character.Name,
                    message,
                    isService: false,
                    DateTime.Now,
                    hasSupporterPack: Api.Client.MasterServer.IsSupporterPackOwner));

            var chatRoomHolder = (ILogicObject)chatRoom.GameObject;
            Instance.CallServer(_ => _.ServerRemote_SendMessage(chatRoomHolder, message));
        }

        public static void ClientSetPrivateChatRead(ChatRoomPrivate privateChatRoom)
        {
            var chatRoomHolder = (ILogicObject)privateChatRoom.GameObject;
            Instance.CallServer(_ => _.ServerRemote_ClientReadPrivateChat(chatRoomHolder));
        }

        public static void ServerAddChatRoomToPlayerScope(ICharacter character, ILogicObject chatRoomHolder)
        {
            if (character.ServerIsOnline)
            {
                Server.World.EnterPrivateScope(character, chatRoomHolder);
                Server.World.ForceEnterScope(character, chatRoomHolder);
            }
        }

        public static ILogicObject ServerCreateChatRoom(BaseChatRoom chatRoom)
        {
            var chatRoomHolder = Server.World.CreateLogicObject<ChatRoomHolder>();
            ChatRoomHolder.ServerSetup(chatRoomHolder, chatRoom);
            return chatRoomHolder;
        }

        [CanBeNull]
        public static ILogicObject ServerFindPrivateChat(ICharacter character, string otherCharacterName)
        {
            if (!ServerPrivateChatRoomsCache.TryGetValue(character, out var currentCharacterChatRooms))
            {
                return null;
            }

            var nameA = character.Name;
            var nameB = otherCharacterName;

            foreach (var existingChatRoomHolder in currentCharacterChatRooms)
            {
                var chatRoom = (ChatRoomPrivate)SharedGetChatRoom(existingChatRoomHolder);
                if ((chatRoom.CharacterA == nameA
                     && chatRoom.CharacterB == nameB)
                    || (chatRoom.CharacterB == nameA
                        && chatRoom.CharacterA == nameB))
                {
                    return existingChatRoomHolder;
                }
            }

            return null;
        }

        public static void ServerRemoveChatRoomFromPlayerScope(ICharacter character, ILogicObject chatRoomHolder)
        {
            if (character.ServerIsOnline)
            {
                Server.World.ForceExitScope(character, chatRoomHolder);
            }
        }

        public static void ServerSendGlobalServiceMessage(string message)
        {
            ServerSendServiceMessage(sharedGlobalChatRoomHolder, message);
        }

        public static void ServerSendServiceMessage(
            ILogicObject chatRoomHolder,
            string message,
            string forCharacterName = null,
            ICharacter customDestinationCharacter = null)
        {
            var charactersDestination = customDestinationCharacter is null
                                            ? SharedGetChatRoom(chatRoomHolder)
                                                .ServerEnumerateMessageRecepients(forPlayer: null)
                                            : new[] { customDestinationCharacter };

            var chatEntry = new ChatEntry(from: forCharacterName,
                                          message,
                                          isService: true,
                                          DateTime.Now,
                                          hasSupporterPack: false);

            Instance.ServerSendMessage(chatRoomHolderObject: chatRoomHolder,
                                       chatEntry,
                                       charactersDestination);
        }

        public static BaseChatRoom SharedGetChatRoom(ILogicObject chatRoomHolder)
        {
            return chatRoomHolder.GetPrivateState<ChatRoomHolder.ChatRoomPrivateState>().ChatRoom;
        }

        [RemoteCallSettings(timeInterval: 1)]
        public ILogicObject ServerRemote_CreatePrivateChatRoom(string inviteeName)
        {
            var currentCharacter = ServerRemoteContext.Character;
            var currentCharacterName = currentCharacter.Name;

            if (inviteeName == currentCharacterName)
            {
                throw new Exception("Cannot create a private chat with self");
            }

            var existingChatRoomHolder = ServerFindPrivateChat(currentCharacter, inviteeName);
            if (existingChatRoomHolder is not null)
            {
                Logger.Warning(
                    $"Private chat room already exist between players {currentCharacter} and {inviteeName}",
                    currentCharacter);

                return existingChatRoomHolder;
            }

            var inviteeCharacter = Server.Characters.GetPlayerCharacter(inviteeName);
            if (inviteeCharacter is null)
            {
                throw new Exception($"Private chat room cannot be created - character not found {inviteeName}");
            }

            // create chat room
            var chatRoomHolder = ServerCreateChatRoom(
                new ChatRoomPrivate(characterA: currentCharacterName,
                                    characterB: inviteeName)
                    { IsClosedByCharacterA = false });

            // register chat room for current character
            ServerAddPrivateChatRoomToCharacterCache(currentCharacter, chatRoomHolder);
            ServerAddPrivateChatRoomToCharacterCache(inviteeCharacter, chatRoomHolder);

            ServerAddChatRoomToPlayerScope(currentCharacter, chatRoomHolder);
            if (currentCharacter != inviteeCharacter)
            {
                ServerAddChatRoomToPlayerScope(inviteeCharacter, chatRoomHolder);
            }

            Logger.Important(
                $"Private chat room created between players {currentCharacter} and {inviteeName}",
                currentCharacter);
            return chatRoomHolder;
        }

        /// <summary>
        /// This method produces player going online/offline notification messages into the chat rooms.
        /// </summary>
        private static void ClientPlayerJoinedOrLeftHandler(OnlinePlayersSystem.Entry entry, bool isOnline)
        {
            if (OnlinePlayersSystem.ClientIsListHidden
                && !ServerOperatorSystem.ClientIsOperator()
                && !ServerModeratorSystem.ClientIsModerator()
                && !PartySystem.ClientIsPartyMember(entry.Name))
            {
                return;
            }

            if (!OnlinePlayersSystem.ClientIsReady
                || sharedGlobalChatRoomHolder is null)
            {
                return;
            }

            var name = entry.Name;
            if (name == Client.Characters.CurrentPlayerCharacter.Name)
            {
                return;
            }

            var message = string.Format(isOnline
                                            ? PlayerWentOnlineChatMessageFormat
                                            : PlayerWentOfflineChatMessageFormat,
                                        name);

            var chatEntry = new ChatEntry(name,
                                          message,
                                          isService: true,
                                          DateTime.Now,
                                          hasSupporterPack: false);

            // display player joined/left notification in global chat only for the party members
            var skipGlobalChat = !PartySystem.ClientIsPartyMember(name);

            if (!skipGlobalChat)
            {
                ClientReceiveChatEntry(
                    chatRoomHolderObject: sharedGlobalChatRoomHolder,
                    chatEntry);
            }

            var allChats = Client.World.GetGameObjectsOfProto<ILogicObject, ChatRoomHolder>();
            foreach (var chatRoomHolder in allChats)
            {
                switch (SharedGetChatRoom(chatRoomHolder))
                {
                    case ChatRoomPrivate privateChatRoom:
                        if (privateChatRoom.CharacterA == name
                            || privateChatRoom.CharacterB == name)
                        {
                            // found a local private chat instance with this character
                            ClientReceiveChatEntry(
                                chatRoomHolderObject: (ILogicObject)privateChatRoom.GameObject,
                                chatEntry);
                        }

                        break;

                    case ChatRoomParty partyChatRoom:
                        if (PartySystem.ClientIsPartyMember(name))
                        {
                            ClientReceiveChatEntry(
                                chatRoomHolderObject: (ILogicObject)partyChatRoom.GameObject,
                                chatEntry);
                        }

                        break;
                }
            }
        }

        private static void ClientReceiveChatEntry(ILogicObject chatRoomHolderObject, ChatEntry chatEntry)
        {
            if (ClientChatBlockList.IsBlocked(chatEntry.From))
            {
                Logger.Info("Received a message from the blocked player: " + chatEntry.From);
                return;
            }

            var chatRoom = SharedGetChatRoom(chatRoomHolderObject);
            chatRoom.ClientOnMessageReceived(chatEntry);

            ClientChatRoomMessageReceived?.Invoke(chatRoom, chatEntry);
        }

        private static void ServerAddPrivateChatRoomToCharacterCache(ICharacter character, ILogicObject chatRoomHolder)
        {
            if (!ServerPrivateChatRoomsCache.TryGetValue(character, out var listPrivateChatRooms))
            {
                listPrivateChatRooms = new List<ILogicObject>();
                ServerPrivateChatRoomsCache[character] = listPrivateChatRooms;
            }

            listPrivateChatRooms.Add(chatRoomHolder);
        }

        private static void ServerLogNewChatEntry(uint chatRoomId, string message, string fromPlayerName)
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            var text = $@"ChatId={chatRoomId} From=""{fromPlayerName}"":{Environment.NewLine}{message}";
            Logger.Important(text);
            Server.Core.AddChatLogEntry(text);
        }

        private static void ServerPlayerNameChangedHandler(string oldName, string newName)
        {
            foreach (var chatRoomHolder in Api.Server.World.GetGameObjectsOfProto<ILogicObject, ChatRoomHolder>())
            {
                if (!(SharedGetChatRoom(chatRoomHolder) is ChatRoomPrivate privateChatRoom))
                {
                    continue;
                }

                if (string.Equals(privateChatRoom.CharacterA, oldName, StringComparison.Ordinal))
                {
                    privateChatRoom.ServerReplaceCharacterName(newName, isCharacterA: true);
                    Logger.Important($"Replaced private chat room username: {oldName}->{newName} in {privateChatRoom}");
                }

                if (string.Equals(privateChatRoom.CharacterB, oldName, StringComparison.Ordinal))
                {
                    privateChatRoom.ServerReplaceCharacterName(newName, isCharacterA: false);
                    Logger.Important($"Replaced private chat room username: {oldName}->{newName} in {privateChatRoom}");
                }
            }
        }

        private static void ServerPlayerOnlineStateChangedHandler(ICharacter playerCharacter, bool isOnline)
        {
            // just write entry into the server log file
            // no need to send it to client (clients are using OnlinePlayersSystem to display these messages)
            var name = playerCharacter.Name;
            var message = string.Format(isOnline
                                            ? PlayerWentOnlineChatMessageFormat
                                            : PlayerWentOfflineChatMessageFormat,
                                        name);

            Server.Core.AddChatLogEntry(message);
        }

        private void ClientRemote_OnMuted(ILogicObject chatRoomHolder, double secondsRemains)
        {
            ClientReceiveChatEntry(
                chatRoomHolder,
                new ChatEntry(from: null,
                              message: string.Format(NotificationCurrentPlayerMuted,
                                                     ClientTimeFormatHelper.FormatTimeDuration(
                                                         secondsRemains)),
                              isService: true,
                              DateTime.Now,
                              hasSupporterPack: false));
        }

        private void ClientRemote_ReceiveMessage(
            [NotNull] ILogicObject chatRoomHolderObject,
            ChatEntry chatEntry)
        {
            if (!chatRoomHolderObject.IsInitialized)
            {
                // probably the chat room was just created (such as party chat room)
                Logger.Warning(
                    $"Chat room is null: {chatRoomHolderObject}. Received message for it: {chatEntry.Message}");
                return;
            }

            ClientReceiveChatEntry(chatRoomHolderObject, chatEntry);
        }

        [RemoteCallSettings(timeInterval: 0.5)]
        private void ServerRemote_ClientClosePrivateChat(ILogicObject chatRoomHolder)
        {
            var character = ServerRemoteContext.Character;
            if (!Server.Core.IsInPrivateScope(character, chatRoomHolder))
            {
                Logger.Error("Player doesn't have access to chat room (not in the private scope): "
                             + chatRoomHolder
                             + " - cannot accept an incoming message here");
                return;
            }

            var chatRoom = (ChatRoomPrivate)SharedGetChatRoom(chatRoomHolder);
            chatRoom.ServerSetReadByCharacter(character);
            chatRoom.ServerSetCloseByCharacter(character);
        }

        [RemoteCallSettings(timeInterval: 1,
                            clientMaxSendQueueSize: 60)]
        private void ServerRemote_ClientReadPrivateChat(ILogicObject chatRoomHolder)
        {
            var character = ServerRemoteContext.Character;
            if (!Server.Core.IsInPrivateScope(character, chatRoomHolder))
            {
                Logger.Error("Player doesn't have access to chat room (not in the private scope): "
                             + chatRoomHolder
                             + " - cannot accept an incoming message here");
                return;
            }

            var chatRoom = (ChatRoomPrivate)SharedGetChatRoom(chatRoomHolder);
            chatRoom.ServerSetReadByCharacter(character);
        }

        [RemoteCallSettings(timeInterval: RemoteCallSettingsAttribute.MaxTimeInterval)]
        private void ServerRemote_RequestChatRooms()
        {
            var character = ServerRemoteContext.Character;

            ServerAddChatRoomToPlayerScope(character, sharedGlobalChatRoomHolder);
            if (ServerIsTradeChatRoomEnabled)
            {
                ServerAddChatRoomToPlayerScope(character, sharedTradeChatRoomHolder);
            }

            ServerAddChatRoomToPlayerScope(character, sharedLocalChatRoomHolder);

            if (ServerPrivateChatRoomsCache.TryGetValue(character, out var characterPrivateChatRooms))
            {
                foreach (var chatRoomHolder in characterPrivateChatRooms)
                {
                    ServerAddChatRoomToPlayerScope(character, chatRoomHolder);
                }
            }
        }

        [RemoteCallSettings(timeInterval: 2,
                            clientMaxSendQueueSize: 20)]
        private void ServerRemote_SendMessage(
            ILogicObject chatRoomHolder,
            string message)
        {
            Api.Assert(chatRoomHolder is not null, "Chat room not found");

            var character = ServerRemoteContext.Character;
            var characterName = character.Name;

            if (ServerPlayerMuteSystem.IsMuted(characterName, out var secondsRemains))
            {
                this.CallClient(character,
                                _ => _.ClientRemote_OnMuted(chatRoomHolder, secondsRemains));
                return;
            }

            if (!Server.Core.IsInPrivateScope(character, chatRoomHolder))
            {
                Logger.Error("Player doesn't have access to chat room (not in the private scope): "
                             + chatRoomHolder
                             + " - cannot accept an incoming message here");
                return;
            }

            if (message.Length > MaxChatEntryLength)
            {
                message = message.Substring(0, MaxChatEntryLength);
            }

            message = ProfanityFilteringSystem.SharedApplyFilters(message);

            var chatRoom = SharedGetChatRoom(chatRoomHolder);

            var chatEntry = new ChatEntry(characterName,
                                          message,
                                          isService: false,
                                          DateTime.Now,
                                          hasSupporterPack: Server.Characters.IsSupporterPackOwner(character));

            chatRoom.ServerAddMessageToLog(chatEntry);

            using var tempDestination = Api.Shared.WrapInTempList(
                chatRoom.ServerEnumerateMessageRecepients(character));
            tempDestination.Remove(character);
            this.ServerSendMessage(chatRoomHolder, chatEntry, tempDestination.AsList());

            ServerLogNewChatEntry(chatRoomHolder.Id, message, characterName);
        }

        private void ServerSendMessage(
            ILogicObject chatRoomHolderObject,
            ChatEntry chatEntry,
            IEnumerable<ICharacter> charactersDestination)
        {
            this.CallClient(charactersDestination,
                            _ => _.ClientRemote_ReceiveMessage(chatRoomHolderObject, chatEntry));
        }

        private class Bootstrapper : BaseBootstrapper
        {
            private const string DatabaseKeyGlobalChatRoomHolder = "GlobalChatRoomHolder";

            private const string DatabaseKeyLocalChatRoomHolder = "LocalChatRoomHolder";

            private const string DatabaseKeyTradeChatRoomHolder = "TradeChatRoomHolder";

            private static readonly IWorldServerService ServerWorld = IsServer ? Api.Server.World : null;

            public override void ClientInitialize()
            {
                ClientChatBlockList.Initialize();
                OnlinePlayersSystem.ClientPlayerAddedOrRemoved += ClientPlayerJoinedOrLeftHandler;
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;

                Refresh();

                static void Refresh()
                {
                    sharedLocalChatRoomHolder = null;
                    sharedGlobalChatRoomHolder = null;
                    sharedTradeChatRoomHolder = null;

                    if (Api.Client.Characters.CurrentPlayerCharacter is not null)
                    {
                        Instance.CallServer(_ => _.ServerRemote_RequestChatRooms());
                    }
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                Server.Characters.PlayerOnlineStateChanged += ServerPlayerOnlineStateChangedHandler;
                Server.Characters.PlayerNameChanged += ServerPlayerNameChangedHandler;
                Server.World.WorldBoundsChanged += ServerLoadSystem;

                ServerLoadSystem();
            }

            private static void ServerLoadSystem()
            {
                ServerPrivateChatRoomsCache.Clear();

                var database = Server.Database;
                if (!database.TryGet(nameof(ChatSystem),
                                     DatabaseKeyGlobalChatRoomHolder,
                                     out sharedGlobalChatRoomHolder))
                {
                    sharedGlobalChatRoomHolder = ServerCreateChatRoom(new ChatRoomGlobal());
                    database.Set(nameof(ChatSystem), DatabaseKeyGlobalChatRoomHolder, sharedGlobalChatRoomHolder);
                }

                if (!database.TryGet(nameof(ChatSystem),
                                     DatabaseKeyTradeChatRoomHolder,
                                     out sharedTradeChatRoomHolder))
                {
                    sharedTradeChatRoomHolder = ServerCreateChatRoom(new ChatRoomTrade());
                    database.Set(nameof(ChatSystem), DatabaseKeyTradeChatRoomHolder, sharedTradeChatRoomHolder);
                }

                if (!database.TryGet(nameof(ChatSystem),
                                     DatabaseKeyLocalChatRoomHolder,
                                     out sharedLocalChatRoomHolder))
                {
                    sharedLocalChatRoomHolder = ServerCreateChatRoom(new ChatRoomLocal());
                    database.Set(nameof(ChatSystem), DatabaseKeyLocalChatRoomHolder, sharedLocalChatRoomHolder);
                }

                // right now it's not possible to enumerate all the existing chat rooms as this is a bootstrapper
                // schedule a delayed initialization
                ServerTimersSystem.AddAction(
                    0.1,
                    () =>
                    {
                        foreach (var chatRoomHolder in ServerWorld
                                                       .GetGameObjectsOfProto<ILogicObject, ChatRoomHolder>()
                                                       .ToList())
                        {
                            if (!(SharedGetChatRoom(chatRoomHolder) is ChatRoomPrivate privateChatRoom))
                            {
                                continue;
                            }

                            var characterA = Server.Characters.GetPlayerCharacter(privateChatRoom.CharacterA);
                            var characterB = Server.Characters.GetPlayerCharacter(privateChatRoom.CharacterB);

                            if (characterA is null
                                || characterB is null)
                            {
                                // incorrect private chat room
                                ServerWorld.DestroyObject(chatRoomHolder);
                                continue;
                            }

                            ServerAddPrivateChatRoomToCharacterCache(characterA, chatRoomHolder);
                            ServerAddPrivateChatRoomToCharacterCache(characterB, chatRoomHolder);
                        }
                    });
            }
        }
    }
}