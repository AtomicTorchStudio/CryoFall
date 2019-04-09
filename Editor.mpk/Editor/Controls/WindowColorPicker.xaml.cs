namespace AtomicTorch.CBND.CoreMod.Editor.Controls
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Controls.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public partial class WindowColorPicker : BaseUserControlWithWindow
    {
        public WindowColorPicker(Color color)
        {
            this.ViewModel = new ViewModelWindowColorPicker(color);
        }

        public Action ColorSelected { get; set; }

        public ViewModelWindowColorPicker ViewModel { get; }

        protected override void WindowClosing()
        {
            try
            {
                if (this.Window.DialogResult == DialogResult.OK)
                {
                    this.ColorSelected?.Invoke();
                }
            }
            finally
            {
                this.DataContext = null;
                this.ViewModel.Dispose();
            }
        }

        protected override void WindowOpening()
        {
            this.DataContext = this.ViewModel;
        }
    }
}