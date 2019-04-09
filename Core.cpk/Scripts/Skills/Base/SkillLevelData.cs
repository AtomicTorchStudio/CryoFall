namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class SkillLevelData : BaseNetObject
    {
        /// <summary>
        /// Please note - this is a Server-side only property, it's not synchronized to client to avoid extra traffic.
        /// To get current experience amount use according client-to-server Skills API.
        /// </summary>
        public double Experience { get; set; }

        [TempOnly]
        [SubscribableProperty]
        public double ExperienceForNextLevel { get; set; }

        [SyncToClient]
        public byte Level { get; set; }
    }
}