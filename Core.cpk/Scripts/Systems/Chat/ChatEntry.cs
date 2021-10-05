namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.ProfanityFiltering;
    using AtomicTorch.CBND.GameApi.Scripting;

    [Serializable]
    public readonly struct ChatEntry
    {
        public readonly string From;

        public readonly bool HasSupporterPack;

        public readonly bool IsService;

        public readonly string Message;

        public readonly DateTime UtcDate;

        public ChatEntry(string from, string message, bool isService, DateTime date, bool hasSupporterPack)
        {
            this.From = from;
            this.Message = message;
            this.IsService = isService;
            this.HasSupporterPack = hasSupporterPack;
            this.UtcDate = date.ToUniversalTime();
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
    }
}