namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelInventorySkeleton : BaseViewModel
    {
        private FrameworkElement control;

        private ViewModelInventorySkeletonViewData inventorySkeletonViewData;

        private bool isActive;

        public FrameworkElement Control
        {
            get => this.control;
            set
            {
                if (this.control == value)
                {
                    return;
                }

                var reactivate = this.IsActive;
                if (reactivate)
                {
                    this.IsActive = false;
                }

                if (this.control is not null)
                {
                    this.control.SizeChanged -= this.ControlSizeChangedHandler;
                    Api.Client.UI.ScreenSizeChanged -= this.UISizeHelperScreenSizeChangedHandler;
                    this.DestroyImageBrushForSkeleton();
                }

                this.control = value;

                if (this.control is not null)
                {
                    this.control.UpdateLayout();
                    this.control.SizeChanged += this.ControlSizeChangedHandler;
                    Api.Client.UI.ScreenSizeChanged += this.UISizeHelperScreenSizeChangedHandler;
                }

                if (reactivate)
                {
                    this.IsActive = true;
                }

                if (this.inventorySkeletonViewData is null
                    && this.control is not null)
                {
                    this.CreateImageBrushForSkeleton();
                }
            }
        }

        public ICharacter CurrentCharacter { get; set; }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;

                if (this.isActive)
                {
                    if (this.control is null)
                    {
                        throw new Exception(
                            $"Cannot activate {nameof(ViewModelInventorySkeleton)} when no {nameof(this.Control)} is assigned");
                    }

                    if (this.inventorySkeletonViewData is null)
                    {
                        this.CreateImageBrushForSkeleton();
                    }

                    this.control.MouseLeftButtonUp += this.ControlMouseLeftButtonUpHandler;
                }
                else
                {
                    this.control.MouseLeftButtonUp -= this.ControlMouseLeftButtonUpHandler;
                }

                if (this.inventorySkeletonViewData is not null)
                {
                    this.inventorySkeletonViewData.IsActive = this.isActive;
                }

                this.NotifyThisPropertyChanged();
            }
        }

        protected override void DisposeViewModel()
        {
            this.DestroyImageBrushForSkeleton();
            this.Control = null;
            this.IsActive = false;
            base.DisposeViewModel();
        }

        private void ControlMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            this.inventorySkeletonViewData?.ToggleView();
        }

        private void ControlSizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            if (this.isActive)
            {
                // recreate image brush for skeleton
                this.CreateImageBrushForSkeleton();
            }
        }

        private void CreateImageBrushForSkeleton()
        {
            this.DestroyImageBrushForSkeleton();

            if (this.control is null)
            {
                return;
            }

            // Alas, we cannot use this method - because the window might be just opening
            // now and the current rendering size is not representing the final rendering size due to scale animation.
            //var renderingSize = Api.Client.UI.CalculateRenderingSize(this.control);

            // So the best solution here is to take the actual width and height and multiply it on UI screen scale coefficient.
            Vector2D renderingSize = (this.control.ActualWidth, this.control.ActualHeight);
            renderingSize *= Api.Client.UI.GetScreenScaleCoefficient();
            // no need to multiple on UI scale as it's already included into screen scale coefficient
            //renderingSize *= Api.Client.UI.Scale;

            if (renderingSize.X <= 0
                || renderingSize.Y <= 0)
            {
                // invalid rendering size
                return;
            }

            this.inventorySkeletonViewData = InventorySkeletonViewHelper.Create(
                this.CurrentCharacter,
                textureWidth: (ushort)Math.Ceiling(renderingSize.X),
                textureHeight: (ushort)Math.Ceiling(renderingSize.Y));

            this.inventorySkeletonViewData.IsActive = this.isActive;

            switch (this.control)
            {
                case Shape s:
                    s.Fill = this.inventorySkeletonViewData.ImageBrush;
                    break;
                case Control c:
                    c.Background = this.inventorySkeletonViewData.ImageBrush;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DestroyImageBrushForSkeleton()
        {
            if (this.inventorySkeletonViewData is null)
            {
                return;
            }

            switch (this.control)
            {
                case Shape s:
                    s.Fill = null;
                    break;

                case Control c:
                    c.Background = null;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.inventorySkeletonViewData.Dispose();
            this.inventorySkeletonViewData = null;
        }

        private void UISizeHelperScreenSizeChangedHandler()
        {
            if (this.isActive)
            {
                // recreate image brush for skeleton
                this.CreateImageBrushForSkeleton();
            }
        }
    }
}