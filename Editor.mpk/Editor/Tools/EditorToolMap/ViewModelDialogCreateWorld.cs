namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolMap
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelDialogCreateWorld : BaseViewModel
    {
        private const int MinWorldSize = 10;

        private ushort offsetX = 10000;

        private ushort offsetY = 10000;

        private ushort sizeX = 100;

        private ushort sizeY = 100;

        public ViewModelDialogCreateWorld()
        {
            this.NormalizeBounds();
        }

        public ushort OffsetX
        {
            get => this.offsetX;
            set
            {
                this.offsetX = value;
                this.NormalizeBounds();
            }
        }

        public ushort OffsetY
        {
            get => this.offsetY;
            set
            {
                this.offsetY = value;
                this.NormalizeBounds();
            }
        }

        public ushort SizeX
        {
            get => this.sizeX;
            set
            {
                this.sizeX = value;
                this.NormalizeBounds();
            }
        }

        public ushort SizeY
        {
            get => this.sizeY;
            set
            {
                this.sizeY = value;
                this.NormalizeBounds();
            }
        }

        public BoundsUshort CreateBounds()
        {
            this.NormalizeBounds();
            return new BoundsUshort(
                minX: this.offsetX,
                minY: this.offsetY,
                maxX: (ushort)(this.offsetX + this.sizeX),
                maxY: (ushort)(this.offsetY + this.sizeY));
        }

        private void NormalizeBounds()
        {
            this.NormalizeSize(ref this.sizeX);
            this.NormalizeSize(ref this.sizeY);
            this.NormalizeOffset(ref this.offsetX, this.sizeX);
            this.NormalizeOffset(ref this.offsetY, this.sizeY);

            this.NotifyPropertyChanged(nameof(this.OffsetX));
            this.NotifyPropertyChanged(nameof(this.OffsetY));
            this.NotifyPropertyChanged(nameof(this.SizeX));
            this.NotifyPropertyChanged(nameof(this.SizeY));
        }

        private void NormalizeOffset(ref ushort offset, ushort size)
        {
            if (offset + size > ushort.MaxValue)
            {
                offset = (ushort)(ushort.MaxValue - size);
            }
        }

        private void NormalizeSize(ref ushort size)
        {
            if (size < MinWorldSize)
            {
                size = MinWorldSize;
            }
        }
    }
}