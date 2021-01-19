namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// Sometimes we make a mistake and the server state requires fixes.
    /// This bootstrapper will apply the necessary fixes during the server startup.
    /// </summary>
    [PrepareOrder(afterType: typeof(BootstrapperServerWorld))]
    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class BootstrapperServerFixes : BaseBootstrapper
    {
        public const int VersionNumber = 1;

        public override void ServerInitialize(IServerConfiguration serverConfiguration)
        {
            if (IsEditor)
            {
                return;
            }

            var db = Api.Server.Database;
            if (!db.TryGet("Core", "ServerFixesVersionNumber", out int actualVersionNumber)
                || VersionNumber != actualVersionNumber)
            {
                db.Set("Core", "ServerFixesVersionNumber", VersionNumber);
                ServerTimersSystem.AddAction(1, ApplyFixes);
            }
        }

        private static void ApplyFixes()
        {
        }
    }
}