namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public partial class WindowAddOrEditServer : BaseUserControlWithWindow
    {
        public const string DialogServerAddressNotValid =
            "Server address is not valid. Please check it.";

        public static readonly DependencyProperty ActionTitleProperty = DependencyProperty.Register(
            nameof(ActionTitle),
            typeof(string),
            typeof(WindowAddOrEditServer),
            new PropertyMetadata("OK"));

        public static readonly DependencyProperty TextAddressProperty = DependencyProperty.Register(
            nameof(TextAddress),
            typeof(string),
            typeof(WindowAddOrEditServer),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty WindowTitleProperty = DependencyProperty.Register(
            nameof(WindowTitle),
            typeof(string),
            typeof(WindowAddOrEditServer),
            new PropertyMetadata("Title"));

        public Action<string> OkAction;

        private Button buttonAction;

        private Button buttonCancel;

        private TextBox textBoxAddress;

        public WindowAddOrEditServer()
        {
        }

        public string ActionTitle
        {
            get => (string)this.GetValue(ActionTitleProperty);
            set => this.SetValue(ActionTitleProperty, value);
        }

        public string TextAddress
        {
            get => (string)this.GetValue(TextAddressProperty);
            set => this.SetValue(TextAddressProperty, value);
        }

        public string WindowTitle
        {
            get => (string)this.GetValue(WindowTitleProperty);
            set => this.SetValue(WindowTitleProperty, value);
        }

        protected override void InitControlWithWindow()
        {
            this.buttonAction = this.GetByName<Button>("ButtonAction");
            this.buttonCancel = this.GetByName<Button>("ButtonCancel");
            this.textBoxAddress = this.GetByName<TextBox>("TextBoxAddress");
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.buttonCancel.Click += this.ButtonCancelClickHandler;
            this.buttonAction.Click += this.ButtonActionClickHandler;
            this.textBoxAddress.PreviewKeyUp += this.TextBoxAddressKeyUpHandler;

            this.textBoxAddress.Focus();
            this.textBoxAddress.CaretIndex = this.TextAddress.Length;
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            this.buttonCancel.Click -= this.ButtonCancelClickHandler;
            this.buttonAction.Click -= this.ButtonActionClickHandler;
            this.textBoxAddress.PreviewKeyUp -= this.TextBoxAddressKeyUpHandler;
        }

        private void ButtonActionClickHandler(object sender, RoutedEventArgs arg1)
        {
            this.DoAction();
        }

        private void ButtonCancelClickHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Window.Close(DialogResult.Cancel);
        }

        private void DoAction()
        {
            var serverName = this.textBoxAddress.Text.TrimNewLinesAndSpaces();
            this.textBoxAddress.SetTextAndMoveCaret(serverName);

            var isError = false;
            try
            {
                if (serverName.Length != 0)
                {
                    var serverUrl = new Uri("game://" + serverName);
                    var parsedServerName = serverUrl.IsDefaultPort
                                               ? serverUrl.Host
                                               : string.Format("{0}:{1}", serverUrl.Host, serverUrl.Port);

                    // regex not work on Mac Mono!
                    //if (!ServerHostPortExpression.IsMatch(serverName))
                    if (!serverName.Equals(parsedServerName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Api.Logger.Error(
                            string.Format(
                                "Parsed server name \"{0}\" not equals to input \"{1}\"",
                                parsedServerName,
                                serverName));
                        isError = true;
                    }
                }
                else
                {
                    isError = true;
                }
            }
            catch (Exception ex)
            {
                Api.Logger.Error(
                    string.Format("Input server name \"{0}\" is incorrect. Error: {1}", serverName, ex.Message));
                isError = true;
            }

            if (isError)
            {
                DialogWindow.ShowDialog(
                    title: null,
                    DialogServerAddressNotValid,
                    closeByEscapeKey: true);
                return;
            }

            this.Window.Close(DialogResult.OK);

            this.OkAction?.Invoke(serverName);
        }

        private void TextBoxAddressKeyUpHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                e.Handled = true;
                this.DoAction();
            }
        }
    }
}