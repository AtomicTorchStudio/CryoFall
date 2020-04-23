namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskKillAny : BasePlayerTaskWithListAndCount<IProtoCharacter>
    {
        public const string DescriptionFormat = "Kill: {0}";

        private TaskKillAny(
            IReadOnlyList<IProtoCharacterMob> protoCharacters,
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

        public static TaskKillAny Require<TProtoCharacterMob>(
            ushort count = 1,
            string description = null)
            where TProtoCharacterMob : class, IProtoCharacterMob
        {
            var list = Api.FindProtoEntities<TProtoCharacterMob>();
            return Require(list, count, description);
        }

        public static TaskKillAny Require(
            IReadOnlyList<IProtoCharacterMob> list,
            ushort count = 1,
            string description = null)
        {
            Api.Assert(list.Count > 0, "Mobs list cannot be empty");
            return new TaskKillAny(list, count, description);
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