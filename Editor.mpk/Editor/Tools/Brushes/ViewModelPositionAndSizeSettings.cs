namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelPositionAndSizeSettings : BaseViewModel
    {
        protected static readonly IWorldClientService WorldService = Api.Client.World;

        private ushort offsetX;

        private ushort offsetXLocal;

        private ushort offsetY;

        private ushort offsetYLocal;

        private ushort sizeX;

        private ushort sizeY;

        public ViewModelPositionAndSizeSettings()
        {
            this.UpdateWorldBounds();
            WorldService.WorldBoundsChanged += this.WorldServiceWorldBoundsChangedHandler;
        }

        public ushort OffsetX
        {
            get => this.offsetX;
            set
            {
                if (this.offsetX == value)
                {
                    return;
                }

                this.offsetX = value;
                this.NotifyThisPropertyChanged();

                this.OnValueChanged();

                this.OffsetXLocal = (ushort)(this.offsetX - this.WorldBoundsOffset.X);
            }
        }

        public ushort OffsetXLocal
        {
            get => this.offsetXLocal;
            set
            {
                if (this.offsetXLocal == value)
                {
                    return;
                }

                this.offsetXLocal = value;
                this.NotifyThisPropertyChanged();

                this.OffsetX = (ushort)(this.offsetXLocal + this.WorldBoundsOffset.X);
            }
        }

        public ushort OffsetY
        {
            get => this.offsetY;
            set
            {
                if (this.offsetY == value)
                {
                    return;
                }

                this.offsetY = value;
                this.NotifyThisPropertyChanged();

                this.OnValueChanged();

                this.OffsetYLocal = (ushort)(this.offsetY - this.WorldBoundsOffset.Y);
            }
        }

        public ushort OffsetYLocal
        {
            get => this.offsetYLocal;
            set
            {
                if (this.offsetYLocal == value)
                {
                    return;
                }

                this.offsetYLocal = value;
                this.NotifyThisPropertyChanged();

                this.OffsetY = (ushort)(this.offsetYLocal + this.WorldBoundsOffset.Y);
            }
        }

        public ushort SizeX
        {
            get => this.sizeX;
            set
            {
                if (this.sizeX == value)
                {
                    return;
                }

                this.sizeX = value;
                this.NotifyThisPropertyChanged();

                this.OnValueChanged();
            }
        }

        public ushort SizeY
        {
            get => this.sizeY;
            set
            {
                if (this.sizeY == value)
                {
                    return;
                }

                this.sizeY = value;
                this.NotifyThisPropertyChanged();

                this.OnValueChanged();
            }
        }

        protected Vector2Ushort WorldBoundsOffset { get; private set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            WorldService.WorldBoundsChanged -= this.WorldServiceWorldBoundsChangedHandler;
        }

        protected virtual void OnValueChanged()
        {
        }

        private void UpdateWorldBounds()
        {
            this.WorldBoundsOffset = IsDesignTime
                                         ? new Vector2Ushort(10000, 10000)
                                         : WorldService.WorldBounds.Offset;
            this.offsetX = this.WorldBoundsOffset.X;
            this.offsetY = this.WorldBoundsOffset.Y;
            this.offsetXLocal = 0;
            this.offsetYLocal = 0;
        }

        private void WorldServiceWorldBoundsChangedHandler()
        {
            this.UpdateWorldBounds();
        }
    }
}