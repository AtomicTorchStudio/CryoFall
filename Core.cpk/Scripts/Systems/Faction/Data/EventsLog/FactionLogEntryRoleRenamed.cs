namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Extensions;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryRoleRenamed : BaseFactionEventLogEntry
    {
        public const string Name = "Role renamed";

        public const string Text_Format
            = "Role [b]{0}[/b] renamed to [b]{1}[/b].";

        public FactionLogEntryRoleRenamed(
            ICharacter byOfficer,
            FactionOfficerRoleTitle previousRoleTitle,
            FactionOfficerRoleTitle newRoleTitle)
            : base(byOfficer)
        {
            this.PreviousRoleTitle = previousRoleTitle;
            this.NewRoleTitle = newRoleTitle;
        }

        public override string ClientText
            => string.Format(Text_Format,
                             this.PreviousRoleTitle.GetDescription(),
                             this.NewRoleTitle.GetDescription());

        public FactionOfficerRoleTitle NewRoleTitle { get; }

        public FactionOfficerRoleTitle PreviousRoleTitle { get; }
    }
}