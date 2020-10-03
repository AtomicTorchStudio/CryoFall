namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Respawn.Data
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;

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

        private readonly string name;

        private readonly IProtoEntity protoEntity;

        public ViewModelDamageSource(
            IProtoEntity protoEntity,
            string name,
            int percent)
            : base(isAutoDisposeFields: false)
        {
            this.name = name;
            this.protoEntity = protoEntity;
            this.Percent = percent;
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

                switch (this.protoEntity)
                {
                    case PlayerCharacter _:
                        return TitlePlayer + ": @" + this.name;

                    case IProtoCharacterMob protoCharacter:
                        return TitleCreature + ": " + protoCharacter.Name;

                    case IProtoObjectExplosive protoExplosive:
                        return TitleExplosion + ": " + protoExplosive.Name;

                    case IProtoStatusEffect protoStatusEffect:
                        return TitleStatusEffect + ": " + protoStatusEffect.Name;

                    case IProtoItem protoItem:
                        return TitleItem + ": " + protoItem.Name;
                    default:
                        return this.protoEntity.Name;
                }
            }
        }
    }
}