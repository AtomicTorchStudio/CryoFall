namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BaseChatRoom : BaseNetObject
    {
        private const int MaxChatHistoryEntries = 50;

        protected BaseChatRoom()
        {
            this.ChatLog = new List<ChatEntry>(capacity: MaxChatHistoryEntries);
        }

        public event Action<ChatEntry> ClientMessageAdded;

        /// <summary>
        /// Please note that only the initial log is replicated.
        /// Client is free to modify it.
        /// Replication of log is not automated and users RPC calls via ChatSystem.
        /// </summary>
        [SyncToClient]
        public List<ChatEntry> ChatLog { get; }

        public abstract string ClientGetTitle();

        public virtual void ClientOnMessageReceived(in ChatEntry chatEntry)
        {
            this.SharedAddMessageToLog(chatEntry);
        }

        public virtual void ServerAddMessageToLog(in ChatEntry chatEntry)
        {
            this.SharedAddMessageToLog(chatEntry);
        }

        public abstract IEnumerable<ICharacter> ServerEnumerateMessageRecepients(ICharacter forPlayer);

        private void SharedAddMessageToLog(ChatEntry chatEntry)
        {
            var log = this.ChatLog;
            while (log.Count + 1 > MaxChatHistoryEntries)
            {
                // trim the log
                log.RemoveAt(0);
            }

            log.Add(chatEntry);

            if (Api.IsClient)
            {
                var handler = this.ClientMessageAdded;
                if (handler is not null)
                {
                    Api.SafeInvoke(() => handler.Invoke(chatEntry));
                }
            }
        }
    }
}