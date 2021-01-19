namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Respawn.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelDamageSource : BaseViewModel
    {
        // prefix for a creature which have dealt the damage
        public const string TitleCreature = "Creature";

        // prefix for an explosion object which have dealt the damage
        public const string TitleExplosion = "Explosion";

        // prefix for an item which have dealt the damage
        public const string TitleItem = "Item";

        // prefix for other damage sources
        // - there are could be many smaller damage sources which we don't list
        public const string TitleOther = "Other";

        // prefix for a player name which have dealt the damage
        public const string TitlePlayer = "Player";

        // prefix for a status effect which have dealt the damage
        public const string TitleStatusEffect = "Status effect";

        private static readonly Brush DefaultBrush = Api.Client.UI.GetApplicationResource<Brush>("BrushColor6");

        private readonly string clanTag;

        private readonly string name;

        private readonly IProtoEntity protoEntity;

        public ViewModelDamageSource(
            IProtoEntity protoEntity,
            string name,
            string clanTag,
            int percent)
            : base(isAutoDisposeFields: false)
        {
            this.name = name;
            this.clanTag = clanTag;
            this.protoEntity = protoEntity;
            this.Percent = percent;
        }

        /// <summary>
        /// The player entry foreground should match that player nickname foreground
        /// in the world when it's not a neutral player.
        /// </summary>
        public Brush Foreground
        {
            get
            {
                if (string.IsNullOrEmpty(this.clanTag)
                    || FactionSystem.ClientCurrentFaction is null)
                {
                    return DefaultBrush;
                }

                var isCurrentFactionMember = FactionSystem.ClientCurrentFactionClanTag == this.clanTag;
                if (isCurrentFactionMember)
                {
                    return ViewModelCharacterNameControl.BrushCurrentFactionMember;
                }

                var status = FactionSystem.ClientGetCurrentFactionDiplomacyStatus(otherFactionClanTag: this.clanTag);
                if (status == FactionDiplomacyStatus.Ally)
                {
                    return ViewModelCharacterNameControl.BrushAllianceMember;
                }

                var isEnemy = status switch
                {
                    FactionDiplomacyStatus.EnemyMutual                   => true,
                    FactionDiplomacyStatus.EnemyDeclaredByCurrentFaction => true,
                    FactionDiplomacyStatus.EnemyDeclaredByOtherFaction   => true,
                    _                                                    => false
                };

                return isEnemy
                           ? ViewModelCharacterNameControl.BrushEnemy
                           : DefaultBrush;
            }
        }

        public int Percent { get; }

        public string Title
        {
            get
            {
                if (this.protoEntity is null)
                {
                    return TitleOther;
                }

                return this.protoEntity switch
                {
                    PlayerCharacter when string.IsNullOrEmpty(this.clanTag)
                        => $"{TitlePlayer}: @{this.name}",
                    PlayerCharacter when !string.IsNullOrEmpty(this.clanTag)
                        => $"{TitlePlayer}: [{this.clanTag}] @{this.name}",
                    IProtoCharacterMob protoCharacter    => TitleCreature + ": " + protoCharacter.Name,
                    IProtoObjectExplosive protoExplosive => TitleExplosion + ": " + protoExplosive.Name,
                    IProtoStatusEffect protoStatusEffect => TitleStatusEffect + ": " + protoStatusEffect.Name,
                    IProtoItem protoItem                 => TitleItem + ": " + protoItem.Name,
                    _                                    => this.protoEntity.Name
                };
            }
        }
    }
}