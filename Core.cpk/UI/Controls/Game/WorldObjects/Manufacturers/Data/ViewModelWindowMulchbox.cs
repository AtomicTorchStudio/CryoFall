namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowMulchbox : ViewModelWindowManufacturer
    {
        private readonly TextureResource infoTextureResource;

        private readonly ObjectMulchboxPrivateState privateState;

        private readonly IProtoObjectMulchbox protoMulchbox;

        public ViewModelWindowMulchbox()
        {
        }

        public ViewModelWindowMulchbox(
            IStaticWorldObject worldObjectManufacturer,
            ObjectMulchboxPrivateState privateState,
            ManufacturingConfig manufacturingConfig,
            TextureResource infoTextureResource)
            : base(
                worldObjectManufacturer,
                privateState,
                manufacturingConfig)
        {
            this.infoTextureResource = infoTextureResource;
            this.protoMulchbox = (IProtoObjectMulchbox)worldObjectManufacturer.ProtoStaticWorldObject;
            this.privateState = privateState;

            this.privateState.ClientSubscribe(
                _ => _.OrganicAmount,
                _ => this.NotifyPropertyChanged(nameof(this.OrganicAmount)),
                this);
        }

        public TextureBrush InfoTextureBrush => Api.Client.UI.GetTextureBrush(this.infoTextureResource);

        public ushort OrganicAmount => this.privateState.OrganicAmount;

        public ushort OrganicCapacity => this.protoMulchbox.OrganicCapacity;
    }
}