namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using JetBrains.Annotations;
    using static FactionDiplomacyStatus;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryDiplomacyStatusChanged : BaseFactionEventLogEntry
    {
        public const string Name = "Diplomacy status changed";

        public const string Text_AllianceAcceptedByOther_Format
            = "[b]\\[{0}\\][/b] accepted our peace treaty proposal.";

        public const string Text_AllianceAcceptedByUs_Format
            = "We have accepted a peace treaty with [b]\\[{0}\\][/b].";

        public const string Text_DeclaredWarByUs_Format
            = "We have declared war on [b]\\[{0}\\][/b].";

        public const string Text_DeclaredWarOnUs_Format
            = "[b]\\[{0}\\][/b] declared war on us.";

        public const string Text_NeutralBrokenAllianceByUs_Format
            = "We have broken alliance with [b]\\[{0}\\][/b].";

        public const string Text_NeutralBrokenAllianceWithUs_Format
            = "[b]\\[{0}\\][/b] has broken alliance with us.";

        public const string Text_NeutralNoLongerDeclareWarByUs_Format
            = "We no longer declare war on [b]\\[{0}\\][/b].";

        public const string Text_NeutralNoLongerDeclareWarOnUs_Format
            = "[b]\\[{0}\\][/b] no longer declares war on us.";

        public const string Text_WarMutualOtherResponded_Format
            = "[b]\\[{0}\\][/b] declared war on us in response to our war declaration on them.";

        public const string Text_WarMutualWeResponded_Format
            = "We have declared war on [b]\\[{0}\\][/b] in response to their war declaration on us.";

        public FactionLogEntryDiplomacyStatusChanged(
            string clanTag,
            [CanBeNull] ICharacter byOfficer,
            FactionDiplomacyStatus fromStatus,
            FactionDiplomacyStatus toStatus)
            : base(byOfficer)
        {
            this.ClanTag = clanTag;
            this.FromStatus = fromStatus;
            this.ToStatus = toStatus;
        }

        public string ClanTag { get; }

        public override NotificationColor ClientNotificationColor
        {
            get
            {
                switch (this.ToStatus)
                {
                    case EnemyMutual:
                    case EnemyDeclaredByCurrentFaction:
                    case EnemyDeclaredByOtherFaction:
                        return NotificationColor.Bad;

                    case Ally:
                        return NotificationColor.Good;

                    case Neutral:
                    default:
                        return NotificationColor.Neutral;
                }
            }
        }

        public override bool ClientShowNotification => true;

        public override string ClientText
        {
            get
            {
                var format = this.ToStatus switch
                {
                    Neutral
                        => this.FromStatus switch
                        {
                            EnemyDeclaredByCurrentFaction => Text_NeutralNoLongerDeclareWarByUs_Format,
                            EnemyDeclaredByOtherFaction   => Text_NeutralNoLongerDeclareWarOnUs_Format,
                            Ally => string.IsNullOrEmpty(this.ByMemberName)
                                        ? Text_NeutralBrokenAllianceWithUs_Format
                                        : Text_NeutralBrokenAllianceByUs_Format,
                            // it's possible only when the faction is dissolved
                            EnemyMutual => Text_NeutralNoLongerDeclareWarOnUs_Format,
                            // impossible cases
                            Neutral => throw new ArgumentOutOfRangeException(),
                            _       => throw new ArgumentOutOfRangeException()
                        },
                    EnemyMutual
                        => this.FromStatus switch
                        {
                            EnemyDeclaredByCurrentFaction => Text_WarMutualOtherResponded_Format,
                            EnemyDeclaredByOtherFaction   => Text_WarMutualWeResponded_Format,
                            // impossible cases
                            _ => throw new ArgumentOutOfRangeException()
                        },
                    EnemyDeclaredByCurrentFaction
                        => this.FromStatus switch
                        {
                            EnemyMutual => Text_NeutralNoLongerDeclareWarOnUs_Format,
                            _           => Text_DeclaredWarByUs_Format
                        },
                    EnemyDeclaredByOtherFaction
                        => this.FromStatus switch
                        {
                            EnemyMutual => Text_NeutralNoLongerDeclareWarByUs_Format,
                            _           => Text_DeclaredWarOnUs_Format
                        },
                    Ally => string.IsNullOrEmpty(this.ByMemberName)
                                ? Text_AllianceAcceptedByOther_Format
                                : Text_AllianceAcceptedByUs_Format,
                    _ => throw new ArgumentOutOfRangeException()
                };

                return string.Format(format, this.ClanTag);
            }
        }

        public FactionDiplomacyStatus FromStatus { get; }

        public FactionDiplomacyStatus ToStatus { get; }
    }
}