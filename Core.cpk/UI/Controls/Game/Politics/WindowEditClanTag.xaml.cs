namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public partial class WindowEditClanTag : BaseUserControlWithWindow
    {
        public static readonly DependencyProperty ClanTagProperty
            = DependencyProperty.Register(nameof(ClanTag),
                                          typeof(string),
                                          typeof(WindowAddOrEditServer),
                                          new PropertyMetadata(string.Empty));

        public Action<string> OkAction;

        private Button btnAction;

        private Button btnCancel;

        private TextBox textBox;

        public string ClanTag
        {
            get => (string)this.GetValue(ClanTagProperty);
            set => this.SetValue(ClanTagProperty, value);
        }

        protected override void InitControlWithWindow()
        {
            this.btnAction = this.GetByName<Button>("ButtonAction");
            this.btnCancel = this.GetByName<Button>("ButtonCancel");
            this.textBox = this.GetByName<TextBox>("TextBox");
        }

        protected override void OnLoaded()
        {
            this.btnCancel.Click += this.BtnCancelOnClick;
            this.btnAction.Click += this.BtnActionOnClick;
            this.textBox.PreviewKeyUp += this.TextBoxKeyUpHandler;

            this.textBox.Focus();
            this.textBox.SetTextAndMoveCaret(this.ClanTag);
        }

        protected override void OnUnloaded()
        {
            this.btnCancel.Click -= this.BtnCancelOnClick;
            this.btnAction.Click -= this.BtnActionOnClick;
            this.textBox.PreviewKeyUp -= this.TextBoxKeyUpHandler;
        }

        private void BtnActionOnClick(object sender, RoutedEventArgs arg1)
        {
            this.DoAction();
        }

        private void BtnCancelOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Window.Close(DialogResult.Cancel);
        }

        private void DoAction()
        {
            var text = this.textBox.Text.TrimNewLinesAndSpaces().ToUpperInvariant();
            this.textBox.SetTextAndMoveCaret(text);
            
            var isError = !PartySystem.SharedIsValidClanTag(text);
            if (isError)
            {
                DialogWindow.ShowDialog(
                    title: CoreStrings.ClanTag_Invalid,
                    text: CoreStrings.ClanTag_Requirements,
                    closeByEscapeKey: true);
                return;
            }

            this.ClanTag = text;
            this.OkAction?.Invoke(text);
        }

        private void TextBoxKeyUpHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                e.Handled = true;
                this.DoAction();
            }
        }
    }
}