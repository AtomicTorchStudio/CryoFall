namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelSkinForCrafting : BaseViewModel
    {
        public ViewModelSkinForCrafting(IProtoItem protoItemSkin)
        {
            this.ProtoItemSkin = protoItemSkin;
        }

        public TextureBrush Icon => Api.Client.UI.GetTextureBrush(this.ProtoItemSkin.Icon);

        public bool IsAvailable => true;

        public IProtoItem ProtoItemSkin { get; }

        public string Title => this.ProtoItemSkin.Name;
    }
}