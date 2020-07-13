namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;

    public class AchievementBuildSteamGenerator : ProtoAchievement
    {
        public override string AchievementId => "build_steam_generator";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskBuildStructure.Require<ObjectGeneratorSteam>());
        }
    }
}