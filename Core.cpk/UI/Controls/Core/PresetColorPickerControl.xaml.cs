namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    /// <summary>
    /// Based on Noesis Color Picker sample updated by @stonstad
    /// https://www.noesisengine.com/forums/viewtopic.php?t=1905#
    /// </summary>
    public partial class PresetColorPickerControl : BaseUserControl
    {
        public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register(
            "Colors",
            typeof(IReadOnlyCollection<Color>),
            typeof(PresetColorPickerControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedColorProperty
            = DependencyProperty.Register("SelectedColor",
                                          typeof(Color),
                                          typeof(PresetColorPickerControl),
                                          new FrameworkPropertyMetadata(
                                              default(Color),
                                              FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private ListBox listBox;

        private Color pickedColor;

        public IReadOnlyCollection<Color> Colors
        {
            get => (IReadOnlyCollection<Color>)this.GetValue(ColorsProperty);
            set => this.SetValue(ColorsProperty, value);
        }

        public Color SelectedColor
        {
            get => (Color)this.GetValue(SelectedColorProperty);
            set => this.SetValue(SelectedColorProperty, value);
        }

        protected override void InitControl()
        {
            this.listBox = this.GetByName<ListBox>("ListBox");
        }

        protected override void OnLoaded()
        {
            this.pickedColor = this.SelectedColor;
            this.listBox.SelectionChanged += this.ListBoxSelectionChangedHandler;
            this.PreviewMouseDown += this.ListBoxPreviewMouseDownHandler;
        }

        protected override void OnUnloaded()
        {
            this.listBox.SelectionChanged -= this.ListBoxSelectionChangedHandler;
            this.PreviewMouseDown -= this.ListBoxPreviewMouseDownHandler;
            this.SelectedColor = this.pickedColor;
        }

        private void ListBoxPreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            // save the picked color
            this.pickedColor = this.SelectedColor;
        }

        private void ListBoxSelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = this.listBox.SelectedValue;
            if (selectedValue is null)
            {
                return;
            }

            // temporary set the pointed color as a selected color to use it as a preview color
            // (the control will revert to the previously selected color or to the picked color in the OnUnloaded method)
            this.SelectedColor = (Color)selectedValue;
        }
    }
}