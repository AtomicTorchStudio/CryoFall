namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Music
{
    using AtomicTorch.CBND.CoreMod.Playlists;
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public static class MainMenuMusicHelper
    {
        private static readonly IClientApi Client = Api.Client;

        private static bool isInitialized;

        public static void Init()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            Client.MasterServer.LoggedInStateChanged += Refresh;
            Client.CurrentGame.ConnectionStateChanged += Refresh;

            Refresh();
        }

        private static void Refresh()
        {
            var shouldPlay = Client.CurrentGame.ConnectionState == ConnectionState.Disconnected
                             && !Client.Core.IsCompiling
                             && !Client.Core.IsCompilationFailed;

            if (!shouldPlay)
            {
                if (ClientMusicSystem.CurrentPlaylist is PlaylistMainMenu)
                {
                    // stop playing main menu playlist
                    ClientMusicSystem.CurrentPlaylist = null;
                }

                return;
            }

            // begin playing main menu playlist
            ClientMusicSystem.CurrentPlaylist = Api.GetProtoEntity<PlaylistMainMenu>();
        }
    }
}