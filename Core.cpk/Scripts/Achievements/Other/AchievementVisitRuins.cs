namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Tiles;

    public class AchievementVisitRuins : ProtoAchievement
    {
        public override string AchievementId => "visit_ruins";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskVisitTile.Require<TileRuins>());
        }
    }
}