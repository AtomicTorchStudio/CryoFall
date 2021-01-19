namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ConstructionMetaInfoEntry : BaseUserControl
    {
        public static readonly DependencyProperty IconFillProperty =
            DependencyProperty.Register("IconFill",
                                        typeof(Brush),
                                        typeof(ConstructionMetaInfoEntry),
                                        new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty IconGeometryProperty =
            DependencyProperty.Register("IconGeometry",
                                        typeof(Geometry),
                                        typeof(ConstructionMetaInfoEntry),
                                        new PropertyMetadata(default(Geometry)));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                                        typeof(string),
                                        typeof(ConstructionMetaInfoEntry),
                                        new PropertyMetadata(default(string)));

        public Brush IconFill
        {
            get => (Brush)this.GetValue(IconFillProperty);
            set => this.SetValue(IconFillProperty, value);
        }

        public Geometry IconGeometry
        {
            get => (Geometry)this.GetValue(IconGeometryProperty);
            set => this.SetValue(IconGeometryProperty, value);
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}