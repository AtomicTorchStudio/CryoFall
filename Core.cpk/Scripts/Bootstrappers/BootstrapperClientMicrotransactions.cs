namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BootstrapperClientMicrotransactions : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            Client.Microtransactions.SkinUnlocked += SkinUnlockedHandler;
        }

        private static void SkinUnlockedHandler(ushort skinId)
        {
            var protoItemSkin = Api.FindProtoEntities<IProtoItemWithSkinData>()
                                   .FirstOrDefault(p => (ushort)p.SkinId == skinId);
            if (protoItemSkin is null)
            {
                Logger.Error("Unknown skin unlocked: skinId=" + skinId);
                return;
            }

            WindowSkinUnlocked.Show(protoItemSkin);
        }
    }
}