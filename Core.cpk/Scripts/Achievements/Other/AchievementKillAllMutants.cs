namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementKillAllMutants : ProtoAchievement
    {
        public override string AchievementId => "kill_all_mutants";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskKill.Require<MobMutantBoar>())
                .Add(TaskKill.Require<MobMutantWolf>())
                .Add(TaskKill.Require<MobMutantHyena>());
        }
    }
}