namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class PlayerCharacterStatistics : BaseNetObject
    {
        [SyncToClient]
        public uint Deaths { get; set; }

        [SyncToClient]
        public uint FarmPlantsHarvested { get; set; }

        [SyncToClient]
        public uint MineralsMined { get; set; }

        public double PvpKillDeathRatio
        {
            get
            {
                var deaths = Math.Max(this.Deaths, 1);
                return this.PvpKills / (double)deaths;
            }
        }

        [SyncToClient]
        public uint PvpKills { get; set; }

        [SyncToClient]
        public double PvpScore { get; set; } = 1.0;

        [TempOnly]
        public double ServerPvpScoreNextRecoveryTime { get; set; }

        [SyncToClient]
        public uint TreesCut { get; set; }
    }
}