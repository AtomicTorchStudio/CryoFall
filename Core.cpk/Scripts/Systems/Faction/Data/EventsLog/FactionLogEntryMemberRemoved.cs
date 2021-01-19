namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryMemberRemoved : BaseFactionEventLogEntry
    {
        public const string Name = "Member removed";

        public const string Text_Format
            = "Officer removed [b]{0}[/b] from the faction.";

        public FactionLogEntryMemberRemoved(
            ICharacter member,
            ICharacter byOfficer)
            : base(byOfficer)
        {
            this.MemberName = member.Name;
        }

        public override string ClientText
        {
            get
            {
                if (string.IsNullOrEmpty(this.ByMemberName))
                {
                    // No officer is responsible for this action.
                    // The member was kicked (probably by the karma system).
                    return string.Format(FactionLogEntryMemberLeft.Text_Format,
                                         this.MemberName)
                           + " "
                           + CoreStrings.Faction_OffendingPlayersAutomaticallyRemoved;
                }

                return string.Format(Text_Format,
                                     this.MemberName);
            }
        }

        public string MemberName { get; }
    }
}