namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryFactionCreated : BaseFactionEventLogEntry
    {
        public const string Name = "Faction created";

        public const string Text = "Faction created.";

        public FactionLogEntryFactionCreated(ICharacter byOfficer)
            : base(byOfficer)
        {
        }

        public override string ClientText => Text;
    }
}