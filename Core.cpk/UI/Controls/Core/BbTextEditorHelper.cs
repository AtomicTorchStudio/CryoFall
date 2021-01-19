namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    public static class BbTextEditorHelper
    {
        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public static void ClientOpenTextEditor(
            string originalText,
            int maxLength,
            int windowHeight,
            Action<string> onSave)
        {
            Grid grid;
            TextBox textBox;
            TextBlock textBlockTextLength;

            {
                var textBlockInfo = new TextBlock()
                {
                    Text = CoreStrings.BbCodeFormattingDescription,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10, 0, 10, 0),
                    FontSize = 11,
                    LineHeight = 12,
                    LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                    FontWeight = FontWeights.Bold
                };

                originalText = FormatTextBeforeEditing(originalText);

                textBox = new TextBox
                {
                    Text = originalText,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    FontSize = 13,
                    TextAlignment = TextAlignment.Left,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    AcceptsReturn = true,
                    Margin = default,
                    BorderThickness = default,
                    Height = windowHeight,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    // max length is clamped anyway by the text length limit logic below
                    MaxLength = maxLength * 2
                };

                textBlockTextLength = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 5, 0, -25),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold
                };

                grid = new Grid()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(7) });
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                grid.Children.Add(textBlockInfo);
                Grid.SetRow(textBox, 2);
                grid.Children.Add(textBox);
                Grid.SetRow(textBlockTextLength, 3);
                grid.Children.Add(textBlockTextLength);
            }

            var dialogWindow = DialogWindow.ShowDialog(
                title: null,
                content: grid,
                okAction: () =>
                          {
                              var text = FormatTextAfterEditing(textBox.Text);
                              onSave(text);
                          },
                cancelAction: () => { },
                okText: CoreStrings.Button_Save);
            dialogWindow.Window.FocusOnControl = textBox;

            dialogWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            dialogWindow.GameWindow.Width = 530;

            var isTextLengthRefreshScheduled = false;
            textBox.PreviewKeyDown += TextBoxKeyDownHandler;
            dialogWindow.Closed += DialogClosedHandler;
            RefreshTextLength();

            void DialogClosedHandler(object sender, EventArgs _)
            {
                dialogWindow.Closed -= DialogClosedHandler;
                textBox.PreviewKeyDown -= TextBoxKeyDownHandler;
            }

            void TextBoxKeyDownHandler(object sender, KeyEventArgs e)
            {
                if (isTextLengthRefreshScheduled)
                {
                    return;
                }

                isTextLengthRefreshScheduled = true;
                ClientTimersSystem.AddAction(0.1, RefreshTextLength);
            }

            void RefreshTextLength()
            {
                isTextLengthRefreshScheduled = false;
                var length = FormatTextAfterEditing(textBox.Text).Length;
                textBlockTextLength.Text = length + "/" + maxLength;
                var isLengthExceeded = length > maxLength;
                dialogWindow.ButtonOk.IsEnabled = !isLengthExceeded;

                textBlockTextLength.Foreground = isLengthExceeded
                                                     ? Brushes.OrangeRed
                                                     : Brushes.White;
            }
        }

        private static string FormatTextAfterEditing(string text)
        {
            return text.Trim()
                       .Replace("\r",     string.Empty)
                       .Replace("\n[br]", "\n")
                       .Replace("\n[*]",  "[*]")
                       .Replace("\n",     "\n[br]")
                       .Replace("[*]",    "\n[*]");
        }

        private static string FormatTextBeforeEditing(string text)
        {
            return text.Trim()
                       .Replace("\r",   string.Empty)
                       .Replace("\n",   string.Empty)
                       .Replace("[br]", "\n")
                       .Replace("[*]",  "\n[*]");
        }
    }
}