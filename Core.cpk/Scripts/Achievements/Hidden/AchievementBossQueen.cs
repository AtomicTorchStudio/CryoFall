namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementBossQueen : ProtoAchievement
    {
        public override string AchievementId => "boss_queen";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskDefeatBoss.Require<MobBossPragmiumQueen>());
        }
    }
}