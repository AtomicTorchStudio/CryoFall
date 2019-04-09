namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ViewModelProtoZone : BaseViewModel
    {
        private readonly IProtoZone zone;

        private bool isRendered;

        private bool isUnderCursor;

        public ViewModelProtoZone(IProtoZone zone)
        {
            this.zone = zone;
        }

        public event Action<ViewModelProtoZone> IsRenderedChanged;

        public Color Color { get; set; }

        public bool IsRendered
        {
            get => this.isRendered;
            set
            {
                // TODO: remove this workaround for the bug when the checkbox marked as checked when hover over it
                if (Api.Client.Input.IsKeyHeld(InputKey.MouseLeftButton))
                {
                    return;
                }

                if (this.isRendered == value)
                {
                    return;
                }

                this.isRendered = value;
                this.NotifyThisPropertyChanged();
                this.IsRenderedChanged?.Invoke(this);
            }
        }

        public bool IsUnderCursor
        {
            get => this.isUnderCursor;
            set
            {
                this.isUnderCursor = value;
                this.VisibilityIsUnderCursor = this.isUnderCursor ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public string Name => this.zone.Name;

        public Visibility VisibilityIsUnderCursor { get; set; }

        public IProtoZone Zone => this.zone;
    }
}