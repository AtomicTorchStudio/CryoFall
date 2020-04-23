namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class AchievementCreaturesKillAll : ProtoAchievement
    {
        public override string AchievementId => "creatures_kill_all";

        protected override void PrepareAchievement(TasksList tasks)
        {
            foreach (var protoMob in Api.FindProtoEntities<IProtoCharacterMob>())
            {
                if (protoMob.IsAvailableInCompletionist)
                {
                    tasks.Add(TaskKill.Require(protoMob));
                }
            }
        }
    }
}