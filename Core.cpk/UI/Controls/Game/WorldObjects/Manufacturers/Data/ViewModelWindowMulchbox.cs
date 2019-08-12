namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowMulchbox : ViewModelWindowManufacturer
    {
        private readonly ObjectMulchboxPrivateState privateState;

        private readonly IProtoObjectMulchbox protoMulchbox;

        public ViewModelWindowMulchbox()
        {
        }

        public ViewModelWindowMulchbox(
            IStaticWorldObject worldObjectManufacturer,
            ObjectMulchboxPrivateState privateState,
            ManufacturingConfig manufacturingConfig)
            : base(
                worldObjectManufacturer,
                privateState,
                manufacturingConfig)
        {
            this.protoMulchbox = (IProtoObjectMulchbox)worldObjectManufacturer.ProtoStaticWorldObject;
            this.privateState = privateState;

            this.privateState.ClientSubscribe(
                _ => _.OrganicAmount,
                _ => this.NotifyPropertyChanged(nameof(this.OrganicAmount)),
                this);
        }

        public ushort OrganicAmount => this.privateState.OrganicAmount;

        public ushort OrganicCapacity => this.protoMulchbox.OrganicCapacity;
    }
}