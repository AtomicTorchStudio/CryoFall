namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEventPsiGrove : ProtoAchievement
    {
        public override string AchievementId => "event_psi_grove";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskKill.Require<MobPsiGrove>());
        }
    }
}