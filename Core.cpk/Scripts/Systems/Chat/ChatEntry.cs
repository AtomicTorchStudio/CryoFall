namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.ProfanityFiltering;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    [Serializable]
    public readonly struct ChatEntry
    {
        public readonly string ClanTag;

        public readonly string From;

        public readonly bool HasSupporterPack;

        public readonly bool IsService;

        public readonly string Message;

        public readonly DateTime UtcDate;

        private ChatEntry(
            string from,
            string message,
            bool isService,
            DateTime date,
            bool hasSupporterPack,
            string clanTag)
        {
            this.From = from;
            this.Message = message;
            this.IsService = isService;
            this.HasSupporterPack = hasSupporterPack;
            this.ClanTag = string.IsNullOrEmpty(clanTag) ? null : clanTag;
            this.UtcDate = date.ToUniversalTime();
        }

        public static ChatEntry CreatePlayerMessage(ICharacter fromCharacter, string message)
        {
            var hasSupporterPack = Api.IsServer
                                       ? Api.Server.Characters.IsSupporterPackOwner(fromCharacter)
                                       : fromCharacter.IsCurrentClientCharacter
                                         && Api.Client.MasterServer.IsSupporterPackOwner;

            return new ChatEntry(fromCharacter.Name,
                                 message,
                                 isService: false,
                                 DateTime.Now,
                                 hasSupporterPack,
                                 clanTag: FactionSystem.SharedGetClanTag(fromCharacter));
        }

        public static ChatEntry CreateServiceMessage(string message, string sourceCharacterName = null)
        {
            return new ChatEntry(from: sourceCharacterName,
                                 message,
                                 isService: true,
                                 DateTime.Now,
                                 hasSupporterPack: false,
                                 clanTag: null);
        }

        public string ClientGetFilteredMessage()
        {
            var message = this.Message;

            var isFromCurrentPlayer = Api.Client.Characters.CurrentPlayerCharacter.Name?.Equals(this.From)
                                      ?? false;
            if (isFromCurrentPlayer)
            {
                // don't apply filtering to the current player's messages
                return message;
            }

            if (Api.Client.SteamApi.IsSteamClient)
            {
                // Apply user-configurable Steam text filters
                // (in some countries there is mandatory filtering).
                // https://store.steampowered.com/news/app/593110/view/2855803154584367415
                message = Api.Client.SteamApi.FilterText(message);
            }

            // Apply our profanity filters (extendable with mods).
            message = ProfanityFilteringSystem.SharedApplyFilters(message);

            return message;
        }

        public ChatEntry WithClanTag(string clanTag)
        {
            return new ChatEntry(
                from: this.From,
                message: this.Message,
                isService: this.IsService,
                date: this.UtcDate,
                hasSupporterPack: this.HasSupporterPack,
                clanTag: clanTag);
        }
    }
}