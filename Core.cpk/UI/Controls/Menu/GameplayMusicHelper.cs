namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu
{
    using AtomicTorch.CBND.CoreMod.Playlists;
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public static class GameplayMusicHelper
    {
        private static bool isInitialized;

        public static void Init()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            Api.Client.CurrentGame.ConnectionStateChanged += Refresh;
            Api.Client.Core.IsCompilingChanged += Refresh;

            Refresh();
        }

        private static void Refresh()
        {
            var shouldPlay = Api.Client.CurrentGame.ConnectionState == ConnectionState.Connected
                             && !Api.Client.Core.IsCompiling
                             && !Api.Client.Core.IsCompilationFailed;

            if (!shouldPlay)
            {
                if (ClientMusicSystem.CurrentPlaylist is PlaylistGameplay)
                {
                    // stop playing gameplay playlist
                    ClientMusicSystem.CurrentPlaylist = null;
                }

                return;
            }

            // begin playing gameplay playlist
            ClientMusicSystem.CurrentPlaylist = Api.GetProtoEntity<PlaylistGameplay>();
        }
    }
}