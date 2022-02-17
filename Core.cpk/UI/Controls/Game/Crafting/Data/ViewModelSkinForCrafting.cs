namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelSkinForCrafting : BaseViewModel
    {
        public ViewModelSkinForCrafting(IProtoItemWithSkinData protoItemSkin)
        {
            this.ProtoItemSkin = protoItemSkin;
        }

        public bool HasSkinCustomEffects => this.ProtoItemSkin.HasSkinCustomEffects;

        public TextureBrush Icon => Api.Client.UI.GetTextureBrush(this.ProtoItemSkin.Icon);

        public bool IsAvailable => true;

        public IProtoItemWithSkinData ProtoItemSkin { get; }

        public string Title => this.ProtoItemSkin.Name;
    }
}