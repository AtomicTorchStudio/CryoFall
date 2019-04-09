namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementKill : QuestRequirementWithList<IProtoCharacter>
    {
        public const string DescriptionFormat = "Kill: {0}";

        private RequirementKill(
            IReadOnlyList<IProtoCharacter> protoCharacters,
            ushort count,
            string description)
            : base(protoCharacters,
                   count,
                   description)
        {
        }

        public override bool IsReversible => false;

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ListNames);

        public static RequirementKill Require<TProtoCharacter>(
            ushort count = 1,
            string description = null)
            where TProtoCharacter : class, IProtoCharacter
        {
            var list = Api.FindProtoEntities<TProtoCharacter>();
            return new RequirementKill(list, count, description);
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
            var context = this.GetActiveContext(attackerCharacter, out var state);
            if (context == null)
            {
                return;
            }

            foreach (var protoCharacter in this.List)
            {
                if (targetCharacter.ProtoCharacter != protoCharacter)
                {
                    // different target type
                    continue;
                }

                state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
                context.Refresh();
                return;
            }
        }
    }
}