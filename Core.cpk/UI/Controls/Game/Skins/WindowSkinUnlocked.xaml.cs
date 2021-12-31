namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowSkinUnlocked : BaseUserControlWithWindow
    {
        private readonly IProtoItemWithSkinData protoItemSkin;

        private ViewModelSkin viewModel;

        private WindowSkinUnlocked(IProtoItemWithSkinData protoItemSkin)
        {
            this.protoItemSkin = protoItemSkin;
        }

        public static void Show(IProtoItemWithSkinData protoItemSkin)
        {
            var window = new WindowSkinUnlocked(protoItemSkin);
            Api.Client.UI.LayoutRootChildren.Add(window);
            window.OpenWindow();
        }

        protected override void OnLoaded()
        {
            this.DataContext
                = this.viewModel = new ViewModelSkin(this.protoItemSkin,
                                                     Api.Client.Microtransactions.GetSkinData(
                                                         (ushort)this.protoItemSkin.SkinId));
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}