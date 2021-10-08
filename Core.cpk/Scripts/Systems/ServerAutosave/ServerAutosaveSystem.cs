namespace AtomicTorch.CBND.CoreMod.Systems.ServerAutosave
{
    using System;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Helpers;

    /// <summary>
    /// This system will perform regular world saves in dedicated server mode.
    /// </summary>
    public class ServerAutosaveSystem : ProtoSystem<ServerAutosaveSystem>
    {
        private const int AutosavegameIntervalMinutesMax = 120;

        private const int AutosavegameIntervalMinutesMin = 1;

        private const double SaveDelaySeconds = 10;

        private static readonly IGameServerService ServerGame = Api.IsServer
                                                                    ? Api.Server.Game
                                                                    : null;

        private static uint serverFramesBetweenAutoSaves;

        private static bool serverIsSavingNow;

        private static uint serverNextAutoSaveFrameNumber;

        public static event Action ClientSaveFinished;

        public static event Action<double> ClientSaveScheduled;

        /// <summary>
        /// Determines whether the autosave system should send a client notifications about the autosaves.
        /// </summary>
        public static bool ServerIsAnnouncingSavegameCreation { get; set; } = true;

        public async void ServerExecuteAutosave()
        {
            if (serverIsSavingNow)
            {
                return;
            }

            try
            {
                serverIsSavingNow = true;

                if (ServerIsAnnouncingSavegameCreation)
                {
                    var scheduledTime = SaveDelaySeconds + ServerGame.FrameTime;

                    Instance.CallClient(
                        Api.Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                        _ => _.ClientRemote_ServerSaveAnnouncement(scheduledTime));
                    await Task.Delay(TimeSpan.FromSeconds(SaveDelaySeconds));
                }

                await Api.Server.World.ServerSaveGamegameAsync();

                if (ServerIsAnnouncingSavegameCreation)
                {
                    Instance.CallClient(
                        Api.Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                        _ => _.ClientRemote_ServerSaveFinished());
                }
            }
            finally
            {
                serverIsSavingNow = false;
            }
        }

        private static void SetNextAutoSaveFrameNumber()
        {
            serverNextAutoSaveFrameNumber = ServerGame.FrameNumber
                                            + serverFramesBetweenAutoSaves;
        }

        private void ClientRemote_ServerSaveAnnouncement(double scheduledTime)
        {
            ClientSaveScheduled?.Invoke(scheduledTime);
        }

        private void ClientRemote_ServerSaveFinished()
        {
            ClientSaveFinished?.Invoke();
        }

        private void Update()
        {
            if (ServerGame.FrameNumber < serverNextAutoSaveFrameNumber)
            {
                return;
            }

            SetNextAutoSaveFrameNumber();
            this.ServerExecuteAutosave();
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

                serverFramesBetweenAutoSaves = 60
                                               * autosaveIntervalMinutes
                                               * ServerGame.FrameRate;

                SetNextAutoSaveFrameNumber();
                // Randomize the next autosave date a bit (up to +60 seconds)
                // to ensure that if there are several game servers they will not save together
                // affecting performance of each other.
                serverNextAutoSaveFrameNumber = (uint)(serverNextAutoSaveFrameNumber
                                                       + ServerGame.FrameRate * RandomHelper.Next(0, 61));

                TriggerEveryFrame.ServerRegister(Instance.Update, "Autosave manager");
            }
        }
    }
}