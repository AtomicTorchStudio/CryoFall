namespace AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    /// <summary>
    /// This system provides current time of day for both client and server.
    /// <see cref="CurrentTimeOfDaySeconds" /> and <see cref="CurrentTimeOfDayHours" />.
    /// There are also two properties: <see cref="IsDay" /> and <see cref="IsNight" />.
    /// </summary>
    public class TimeOfDaySystem : ProtoSystem<TimeOfDaySystem>
    {
        /// <summary>
        /// Duration of one game day in game time.
        /// </summary>
        public const double GameDayDurationGameSeconds
            = GameDayDurationRealSeconds * 60; // one game day is 86400 game seconds.

        /// <summary>
        /// Duration of one game day in real time.
        /// </summary>
        public const double GameDayDurationRealSeconds
            = GameDayHoursCount * 60; // one game day is 24 real time minutes or 1440 real time seconds.

        /// <summary>
        /// How many hours in one game day.
        /// Please don't change unless you also modify the time intervals for dawn/day/dusk/night below.
        /// </summary>
        public const int GameDayHoursCount = 24;

        public static readonly GameTimeInterval IntervalDawn = new(fromHour: 4, toHour: 8);

        public static readonly GameTimeInterval IntervalDay = new(fromHour: 6, toHour: 21);

        public static readonly GameTimeInterval IntervalDusk = new(fromHour: 19, toHour: 23);

        public static readonly GameTimeInterval IntervalNight = new(fromHour: 21, toHour: 6);

        private static readonly TimeSpan InitialServerTimeOfDay = new(hours: 12, minutes: 0, seconds: 0);

        private static double serverTimeOfDayOffsetSeconds;

        public static bool ClientIsInitialized { get; private set; }

        public static double CurrentTimeOfDayHours => CurrentTimeOfDaySeconds / (60 * 60);

        public static double CurrentTimeOfDaySeconds
        {
            get
            {
                if (IsClient 
                    && !ClientIsInitialized)
                {
                    return 0;
                }

                var realTime = Api.IsServer
                                   ? Api.Server.Game.FrameTime
                                   : Api.Client.CurrentGame.ServerFrameTimeApproximated;

                // offset time
                realTime += serverTimeOfDayOffsetSeconds;

                // game day time fraction (from 0 to 1)
                var gameTime = realTime % GameDayDurationRealSeconds / GameDayDurationRealSeconds;

                // gameTime * seconds per minute * minutes per hours * hours per day
                return gameTime * 60.0 * 60.0 * GameDayHoursCount;
            }
        }

        public static double DawnFraction => IntervalDawn.CalculateCurrentFraction(CurrentTimeOfDayHours);

        public static double DayFraction => IntervalDay.CalculateCurrentFraction(CurrentTimeOfDayHours);

        public static double DuskFraction => IntervalDusk.CalculateCurrentFraction(CurrentTimeOfDayHours);

        public static bool IsDay => !IsNight;

        public static bool IsNight => NightFraction > 0.33;

        public static double NightFraction => IntervalNight.CalculateCurrentFraction(CurrentTimeOfDayHours);

        public static double ServerTimeOfDayOffsetSeconds => serverTimeOfDayOffsetSeconds;

        public override string Name => "Day-night cycle system";

        public static void ServerResetTimeOfDayOffset()
        {
            ServerSetCurrentTimeOfDay(
                (byte)InitialServerTimeOfDay.Hours,
                (byte)InitialServerTimeOfDay.Minutes);
        }

        public static void ServerSetCurrentTimeOfDay(byte hour, byte minute)
        {
            if (minute >= 60)
            {
                throw new Exception("Minute cannot be >= 60");
            }

            hour %= GameDayHoursCount;

            // calculate desired time of day
            var desiredTimeOfDay = 60 * 60 * (hour + minute / 60.0);

            // calculate current time of day (not offsetted!)
            // (using algorithm similar to the CurrentTimeOfDaySeconds property except the offset part)
            var realTime = Api.Server.Game.FrameTime;
            // game day time fraction (from 0 to 1)
            var gameTime = realTime % GameDayDurationRealSeconds / GameDayDurationRealSeconds;
            // gameTime * seconds per minute * minutes per hours * hours per day
            var currentTimeOfDay = gameTime * 60.0 * 60.0 * GameDayHoursCount;

            // calculate offset
            var offsetGameSeconds = desiredTimeOfDay - currentTimeOfDay;
            if (offsetGameSeconds < 0)
            {
                // add one full day duration
                offsetGameSeconds += GameDayDurationGameSeconds;
            }

            var offsetRealSeconds = offsetGameSeconds / 60;

            ServerSetTimeOfDayOffsetSeconds(offsetRealSeconds);

            //// logging to help investigate the parameters above
            //Logger.WriteDev(
            //    string.Format(
            //        "Applying delta time offset:"
            //        + "{0}Delta (game seconds): {1:F0}s"
            //        + "{0}Delta (real seconds): {2:F0}s"
            //        + "{0}Current time of day (game seconds): {3:F0}s"
            //        + "{0}Desired time of day (game seconds): {4:F0}s"
            //        + "{0}Current time of day with the offset (result game seconds): {5:F0}s",
            //        "   " + Environment.NewLine,
            //        offsetGameSeconds,
            //        offsetRealSeconds,
            //        currentTimeOfDay,
            //        desiredTimeOfDay,
            //        CurrentTimeOfDaySeconds));
        }

        protected override void PrepareSystem()
        {
            if (IsServer)
            {
                if (Api.Server.Database.TryGet(
                    nameof(TimeOfDaySystem),
                    nameof(ServerTimeOfDayOffsetSeconds),
                    out double savedOffsetSeconds))
                {
                    // restore saved offset
                    ServerSetTimeOfDayOffsetSeconds(savedOffsetSeconds);
                }
                else
                {
                    // set initial time
                    ServerResetTimeOfDayOffset();
                }
            }
            else // if client
            {
                Api.Client.Characters.CurrentPlayerCharacterChanged += Refresh;
                Refresh();

                void Refresh()
                {
                    ClientIsInitialized = false;

                    if (Api.Client.Characters.CurrentPlayerCharacter is not null)
                    {
                        this.CallServer(_ => _.ServerRemote_RequestGetTimeOfDayOffset());
                    }
                }
            }
        }

        private static void ServerSetTimeOfDayOffsetSeconds(double offsetSeconds)
        {
            Api.ValidateIsServer();

            serverTimeOfDayOffsetSeconds = offsetSeconds;
            Api.Server.Database.Set(nameof(TimeOfDaySystem), nameof(ServerTimeOfDayOffsetSeconds), offsetSeconds);

            SharedWriteToLogCurrentTimeOfDay();

            // notify clients about new time offset
            Instance.CallClient(
                Api.Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                _ => _.ClientRemote_ServerTimeOfDayOffsetUpdate(offsetSeconds));
        }

        private static void SharedWriteToLogCurrentTimeOfDay()
        {
            Logger.Important(
                "Server time-of-day offset set:"
                + Environment.NewLine
                + "Current time offset: "
                + TimeSpan.FromSeconds(serverTimeOfDayOffsetSeconds)
                + Environment.NewLine
                + "Current time of day: "
                + TimeSpan.FromSeconds(CurrentTimeOfDaySeconds));
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_ServerTimeOfDayOffsetUpdate(double offsetSeconds)
        {
            // Please note: no need to correct the offsetSeconds by RTT/2 time
            // because this correction is done automatically when current time of day is requested.
            serverTimeOfDayOffsetSeconds = offsetSeconds;
            ClientIsInitialized = true;
            SharedWriteToLogCurrentTimeOfDay();
        }

        [RemoteCallSettings(timeInterval: RemoteCallSettingsAttribute.MaxTimeInterval)]
        private void ServerRemote_RequestGetTimeOfDayOffset()
        {
            this.CallClient(
                ServerRemoteContext.Character,
                _ => _.ClientRemote_ServerTimeOfDayOffsetUpdate(serverTimeOfDayOffsetSeconds));
        }
    }
}