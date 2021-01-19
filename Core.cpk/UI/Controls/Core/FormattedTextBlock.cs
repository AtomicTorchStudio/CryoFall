namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class FormattedTextBlock : BaseContentControl
    {
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register(nameof(LineHeight),
                                        typeof(double),
                                        typeof(FormattedTextBlock),
                                        new PropertyMetadata(default(double)));

        public static readonly DependencyProperty LineStackingStrategyProperty =
            DependencyProperty.Register(nameof(LineStackingStrategy),
                                        typeof(LineStackingStrategy),
                                        typeof(FormattedTextBlock),
                                        new PropertyMetadata(default(LineStackingStrategy)));

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(nameof(TextAlignment),
                                        typeof(TextAlignment),
                                        typeof(FormattedTextBlock),
                                        new PropertyMetadata(default(TextAlignment)));

        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register(nameof(TextTrimming),
                                        typeof(TextTrimming),
                                        typeof(FormattedTextBlock),
                                        new PropertyMetadata(defaultValue: TextTrimming.None));

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register(nameof(TextWrapping),
                                        typeof(TextWrapping),
                                        typeof(FormattedTextBlock),
                                        new PropertyMetadata(defaultValue: TextWrapping.Wrap));

        private static readonly Lazy<ControlTemplate> LazyDefaultTemplate
            = new(() => Api.Client.UI.GetApplicationResource<ControlTemplate>(
                    "DefaultFormattedTextBlockTemplate"));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text),
                                        typeof(string),
                                        typeof(FormattedTextBlock),
                                        new PropertyMetadata(null,
                                                             ContentOrTextPropertyChangedHandler));

        private TextBlock textBlock;

        static FormattedTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(FormattedTextBlock),
                new FrameworkPropertyMetadata(typeof(FormattedTextBlock)));

            ContentProperty.OverrideMetadata(
                typeof(FormattedTextBlock),
                new PropertyMetadata(defaultValue: null,
                                     ContentOrTextPropertyChangedHandler));
        }

        public double LineHeight
        {
            get => (double)this.GetValue(LineHeightProperty);
            set => this.SetValue(LineHeightProperty, value);
        }

        public LineStackingStrategy LineStackingStrategy
        {
            get => (LineStackingStrategy)this.GetValue(LineStackingStrategyProperty);
            set => this.SetValue(LineStackingStrategyProperty, value);
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public TextAlignment TextAlignment
        {
            get => (TextAlignment)this.GetValue(TextAlignmentProperty);
            set => this.SetValue(TextAlignmentProperty, value);
        }

        public TextTrimming TextTrimming
        {
            get => (TextTrimming)this.GetValue(TextTrimmingProperty);
            set => this.SetValue(TextTrimmingProperty, value);
        }

        public TextWrapping TextWrapping
        {
            get => (TextWrapping)this.GetValue(TextWrappingProperty);
            set => this.SetValue(TextWrappingProperty, value);
        }

        protected override void InitControl()
        {
            if (VisualTreeHelper.GetChildrenCount(this) == 0)
            {
                // set default template
                this.Template = LazyDefaultTemplate.Value;
            }
        }

        protected override void OnLoaded()
        {
            this.textBlock = (TextBlock)VisualTreeHelper.GetChild(this, 0);
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.textBlock?.Inlines.Clear();
            this.textBlock = null;
        }

        private static void ContentOrTextPropertyChangedHandler(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((FormattedTextBlock)d).Refresh();
        }

        private void Refresh()
        {
            if (!this.isLoaded)
            {
                return;
            }

            this.textBlock.Inlines.Clear();

            var text = this.Text;
            var contentValue = this.Content;

            if (!string.IsNullOrEmpty(text)
                && contentValue is not null)
            {
                Api.Logger.Error(
                    string.Format(
                        "Both Text and Content properties are defined for {0}. Only a single property should be defined.{1}Text: {2}{1}Content: {3}",
                        nameof(FormattedTextBlock),
                        Environment.NewLine,
                        text,
                        contentValue));
            }

            if (string.IsNullOrEmpty(text))
            {
                text = Convert.ToString(contentValue);
            }

            if (string.IsNullOrEmpty(text))
            {
                return;

                //Api.Logger.Error("No content defined for " + nameof(FormattedTextBlock));
                //// ReSharper disable once CanExtractXamlLocalizableStringCSharp
                //text = "<No content defined>";
            }

            this.textBlock.Inlines.AddRange(ClientTextTagFormatter.ParseInlines(text));
        }
    }
}