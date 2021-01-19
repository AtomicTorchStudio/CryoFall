namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryDescriptionChanged : BaseFactionEventLogEntry
    {
        public const string Name = "Faction description changed";

        public const string Text = "Faction description changed.";

        public FactionLogEntryDescriptionChanged(ICharacter byOfficer) : base(byOfficer)
        {
        }

        public override string ClientText => Text;
    }
}