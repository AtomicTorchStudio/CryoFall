namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelLocationSettings : ViewModelPositionAndSizeSettings
    {
        public readonly BaseClientComponentEditorToolSelectLocation ComponentSelectLocation;

        private bool isUpdatingProperties;

        public ViewModelLocationSettings(BaseClientComponentEditorToolSelectLocation componentSelectLocation)
        {
            this.ComponentSelectLocation = componentSelectLocation;
            this.ComponentSelectLocation.SelectionBoundsChanged +=
                this.ComponentSelectLocationSelectionBoundsChangedHandler;

            this.UpdateProperties();
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.ComponentSelectLocation.SelectionBoundsChanged -=
                this.ComponentSelectLocationSelectionBoundsChangedHandler;
        }

        protected override void OnValueChanged()
        {
            if (!this.isUpdatingProperties)
            {
                this.SetBoundsFromProperties();
            }
        }

        protected void UpdateProperties()
        {
            this.isUpdatingProperties = true;

            if (IsDesignTime)
            {
                this.OffsetX = 10100;
                this.OffsetY = 10200;
                this.SizeX = 100;
                this.SizeY = 200;
            }
            else
            {
                var selectionBounds = this.ComponentSelectLocation.SelectionBounds;
                var offset = selectionBounds.Offset;
                offset = new Vector2Ushort(Math.Max(offset.X, this.WorldBoundsOffset.X),
                                           Math.Max(offset.Y, this.WorldBoundsOffset.Y));

                this.OffsetX = offset.X;
                this.OffsetY = offset.Y;
                this.SizeX = selectionBounds.Size.X;
                this.SizeY = selectionBounds.Size.Y;
            }

            this.isUpdatingProperties = false;
        }

        private void ComponentSelectLocationSelectionBoundsChangedHandler()
        {
            this.UpdateProperties();
        }

        private void SetBoundsFromProperties()
        {
            this.ComponentSelectLocation.SetSelectionBounds(
                new BoundsUshort(
                    offset: (this.OffsetX, this.OffsetY),
                    size: (this.SizeX, this.SizeY)));
        }
    }
}