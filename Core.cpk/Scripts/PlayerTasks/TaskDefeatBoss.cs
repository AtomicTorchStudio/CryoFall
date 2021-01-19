namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Systems.BossLootSystem;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TaskDefeatBoss : BasePlayerTaskWithCount<PlayerTaskStateWithCount>
    {
        public const string DescriptionFormat = "Defeat boss: {0}";

        private TaskDefeatBoss(
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

        public static TaskDefeatBoss Require<TProtoCharacterBoss>(
            ushort count = 1,
            string description = null)
            where TProtoCharacterBoss : IProtoCharacterBoss, new()
        {
            var protoCharacterMob = Api.GetProtoEntity<TProtoCharacterBoss>();
            return Require(protoCharacterMob, count, description);
        }

        public static TaskDefeatBoss Require(
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
                ServerBossLootSystem.BossDefeated += this.BossDefeatedHandler;
            }
            else
            {
                ServerBossLootSystem.BossDefeated -= this.BossDefeatedHandler;
            }
        }

        private void BossDefeatedHandler(
            IProtoCharacterMob protoCharacterBoss,
            Vector2Ushort bossPosition,
            List<ServerBossLootSystem.WinnerEntry> winnerEntries)
        {
            if (protoCharacterBoss != this.ProtoCharacterMob)
            {
                // different boss type
                return;
            }

            foreach (var winner in winnerEntries)
            {
                var activeContext = this.GetActiveContext(winner.Character, out var state);
                if (activeContext is null)
                {
                    continue;
                }

                state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
                activeContext.Refresh();
            }
        }
    }
}