namespace AtomicTorch.CBND.CoreMod.Helpers.Server
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ServerSpawnRateScaleHelper
    {
        /// <summary>
        /// Apply current spawn rate adjustment coefficient based on the number of online players.
        /// For example, if number of players closer to 200, it will apply x2 rate reducing the duration in half.
        /// </summary>
        public static double AdjustDurationByRate(double value)
        {
            var rate = CalculateCurrentRate();
            if (rate > 1)
            {
                value /= rate;
            }

            return value;
        }

        /// <summary>
        /// Calculate current spawn rate adjustment coefficient based on the number of online players.
        /// </summary>
        public static double CalculateCurrentRate()
        {
            var playersOnlineCount = Api.Server.Characters.OnlinePlayersCount;
            double minPlayers = 50,
                   maxPlayers = 200;

            if (playersOnlineCount <= minPlayers)
            {
                return 1.0; // normal rate
            }

            if (playersOnlineCount >= maxPlayers)
            {
                return 2.0; // max rate;
            }

            // increase linearly
            return 1.0 + (playersOnlineCount - minPlayers) / (maxPlayers - minPlayers);
        }
    }
}