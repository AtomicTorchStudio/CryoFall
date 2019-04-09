namespace AtomicTorch.CBND.CoreMod.Systems.ServerAutosave
{
    using System;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    /// <summary>
    /// Do regular saves in dedicated server mode.
    /// </summary>
    public static class ServerAutosaveSystem
    {
        public const string NotificationWorldBackupAnnouncement
            = "SERVER: World backup is starting in {0} seconds.";

        public const string NotificationWorldBackupComplete
            = "SERVER: World backup is complete!";

        private const int AutosavegameIntervalMinutesMax = 60;

        private const int AutosavegameIntervalMinutesMin = 1;

        private const int SaveDelaySeconds = 10;

        private static readonly IGameServerService ServerGame = Api.IsServer
                                                                    ? Api.Server.Game
                                                                    : null;

        private static uint framesBetweenAutoSaves;

        private static bool isSavingNow;

        private static uint nextAutoSaveFrameNumber;

        /// <summary>
        /// Determines whether the autosave system should broadcast a service chat message notifying about the savegame creation.
        /// </summary>
        public static bool IsAnnouncingSavegameCreation { get; set; } = true;

        private static async void ExecuteAutosave()
        {
            if (isSavingNow)
            {
                return;
            }

            try
            {
                isSavingNow = true;

                if (IsAnnouncingSavegameCreation)
                {
                    ChatSystem.ServerSendGlobalServiceMessage(
                        string.Format(NotificationWorldBackupAnnouncement, SaveDelaySeconds));
                    await Task.Delay(TimeSpan.FromSeconds(SaveDelaySeconds));
                }

                await Api.Server.World.ServerSaveGamegameAsync();

                if (IsAnnouncingSavegameCreation)
                {
                    ChatSystem.ServerSendGlobalServiceMessage(NotificationWorldBackupComplete);
                }
            }
            finally
            {
                isSavingNow = false;
            }
        }

        private static void SetNextAutoSaveFrameNumber()
        {
            nextAutoSaveFrameNumber = ServerGame.FrameNumber
                                      + framesBetweenAutoSaves;
        }

        private static void Update()
        {
            if (ServerGame.FrameNumber < nextAutoSaveFrameNumber)
            {
                return;
            }

            SetNextAutoSaveFrameNumber();
            ExecuteAutosave();
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                if (Api.IsEditor)
                {
                    return;
                }

                var autosaveIntervalMinutes = Api.Server.Core.AutosaveIntervalMinutes;
                if (autosaveIntervalMinutes < AutosavegameIntervalMinutesMin
                    || autosaveIntervalMinutes > AutosavegameIntervalMinutesMax)
                {
                    throw new Exception(
                        string.Format("Autosave interval should be in range from {0} to {1} (both inclusive)",
                                      AutosavegameIntervalMinutesMin,
                                      AutosavegameIntervalMinutesMax));
                }

                Server.Core.AutosaveOnQuit = true;
                Api.Logger.Important($"Server auto-save enabled. Interval: {autosaveIntervalMinutes} minutes");

                framesBetweenAutoSaves = 60
                                         * autosaveIntervalMinutes
                                         * ServerGame.FrameRate;

                SetNextAutoSaveFrameNumber();
                TriggerEveryFrame.ServerRegister(Update, "Autosave manager");
            }
        }
    }
}