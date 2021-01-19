namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskKill : BasePlayerTaskWithCount<PlayerTaskStateWithCount>
    {
        public const string DescriptionFormat = "Kill: {0}";

        private TaskKill(
            IProtoCharacterMob protoCharacterMob,
            ushort count,
            string description)
            : base(count,
                   description)
        {
            this.ProtoCharacterMob = protoCharacterMob;
        }

        public override bool IsReversible => false;

        public IProtoCharacterMob ProtoCharacterMob { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ProtoCharacterMob.Name);

        public static TaskKill Require<TProtoCharacterMob>(
            ushort count = 1,
            string description = null)
            where TProtoCharacterMob : IProtoCharacterMob, new()
        {
            var protoCharacterMob = Api.GetProtoEntity<TProtoCharacterMob>();
            return Require(protoCharacterMob, count, description);
        }

        public static TaskKill Require(
            IProtoCharacterMob protoCreature,
            ushort count = 1,
            string description = null)
        {
            return new(protoCreature, count, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.ProtoCharacterMob.Icon;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                ServerCharacterDeathMechanic.CharacterKilled += this.CharacterKilledHandler;
            }
            else
            {
                ServerCharacterDeathMechanic.CharacterKilled -= this.CharacterKilledHandler;
            }
        }

        private void CharacterKilledHandler(ICharacter attackerCharacter, ICharacter targetCharacter)
        {
            if (attackerCharacter.IsNpc)
            {
                return;
            }

            var context = this.GetActiveContext(attackerCharacter, out var state);
            if (context is null)
            {
                return;
            }

            if (targetCharacter.ProtoCharacter != this.ProtoCharacterMob)
            {
                // different target type
                return;
            }

            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            context.Refresh();
        }
    }
}