namespace AtomicTorch.CBND.CoreMod.Helpers.Server
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ServerLocalModeHelper
    {
        public static bool IsLocalServer
        {
            get
            {
                Api.ValidateIsServer();
                return Api.Server.Core.GetLaunchArgumentValue("+localServer") == "1";
            }
        }
    }
}