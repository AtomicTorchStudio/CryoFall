namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    // Currently the data here is not synchronized to the client
    // and can be browsed by the server operator with console commands.
    public class PlayerCharacterStatistics : BaseNetObject
    {
        public uint Deaths { get; set; }

        public uint MineralsMined { get; set; }

        public double PvpKillDeathRatio
        {
            get
            {
                var deaths = Math.Max(this.Deaths, 1);
                return this.PvpKills / (double)deaths;
            }
        }

        public uint PvpKills { get; set; }

        public double PvpScore { get; set; } = 1.0;

        [TempOnly]
        public double ServerPvpScoreNextRecoveryTime { get; set; }

        public uint TreesCut { get; set; }
    }
}