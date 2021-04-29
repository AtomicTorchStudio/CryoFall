namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryFactionBaseRaided : BaseFactionEventLogEntry
    {
        public const string Name = "Faction base raided";

        public const string Text_Format
            = "Our base at {0} is raided by [b]{1}[/b].";

        public FactionLogEntryFactionBaseRaided(Vector2Ushort basePosition, [CanBeNull] ICharacter raiderCharacter)
            : base(byMember: null)
        {
            if (raiderCharacter is not null)
            {
                this.RaiderPlayerName = raiderCharacter.Name;
                this.RaiderClanTag = FactionSystem.SharedGetClanTag(raiderCharacter);
            }

            this.BasePosition = basePosition;
        }

        public Vector2Ushort BasePosition { get; }

        public override string ClientText
        {
            get
            {
                string message;
                if (string.IsNullOrEmpty(this.RaiderClanTag))
                {
                    message = this.RaiderPlayerName;
                }
                else
                {
                    message = string.Format(CoreStrings.ClanTag_FormatWithName,
                                            this.RaiderClanTag,
                                            this.RaiderPlayerName)
                                    // escape formatting
                                    .Replace("[", @"\[")
                                    .Replace("]", @"\]");
                }

                return string.Format(Text_Format,
                                     this.BasePosition - Api.Client.World.WorldBounds.Offset,
                                     message);
            }
        }

        public string RaiderClanTag { get; }

        public string RaiderPlayerName { get; }
    }
}