namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class DialogWindow : BaseContentControl
    {
        public static readonly DependencyProperty TitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(TitleFontSize),
                typeof(double),
                typeof(DialogWindow),
                new PropertyMetadata(10));

        public static readonly DependencyProperty WindowTitleProperty = DependencyProperty.Register(
            nameof(WindowTitle),
            typeof(string),
            typeof(DialogWindow),
            new PropertyMetadata(string.Empty));

        private static readonly Action EmptyAction = () => { };

        public Action CancelAction;

        public Action OkAction;

        private Button btnCancel;

        private Button btnOk;

        private bool closeByEscapeKey;

        private GameWindow window;

        private int zIndexOffset;

        static DialogWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DialogWindow),
                new FrameworkPropertyMetadata(typeof(DialogWindow)));
        }

        public event EventHandler Closed;

        public string CancelText { get; private set; }

        public bool CloseByEscapeKey
        {
            get => this.closeByEscapeKey;
            set
            {
                this.closeByEscapeKey = value;
                if (this.window != null)
                {
                    this.window.CloseByEscapeKey = value;
                }
            }
        }

        public DialogResult DialogResult => this.window.DialogResult;

        public bool FocusOnCancelButton { get; private set; }

        public GameWindow GameWindow => this.window;

        public bool HideOkButton { get; private set; }

        public string OkText { get; private set; }

        public double TitleFontSize
        {
            get => (double)this.GetValue(TitleFontSizeProperty);
            set => this.SetValue(TitleFontSizeProperty, value);
        }

        public GameWindow Window => this.window;

        public string WindowTitle
        {
            get => (string)this.GetValue(WindowTitleProperty);
            set => this.SetValue(WindowTitleProperty, value);
        }

        public int ZIndexOffset
        {
            get => this.zIndexOffset;
            set
            {
                this.zIndexOffset = value;
                this.UpdateWindowZIndexOffset();
            }
        }

        public static FrameworkElement CreateTextElement(string text, TextAlignment textAlignment)
        {
            return new FormattedTextBlock
            {
                Content = text,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = textAlignment
            };
        }

        public static DialogWindow ShowDialog(
            string title,
            string text,
            TextAlignment textAlignment = TextAlignment.Left,
            Action okAction = null,
            Action cancelAction = null,
            bool hideOkButton = false,
            bool hideCancelButton = true,
            bool focusOnCancelButton = false,
            string okText = null,
            string cancelText = null,
            bool closeByEscapeKey = false,
            int zIndexOffset = 0,
            bool autoWidth = false)
        {
            return ShowDialog(
                title,
                CreateTextElement(text, textAlignment),
                okAction,
                cancelAction,
                hideOkButton,
                hideCancelButton,
                focusOnCancelButton,
                okText,
                cancelText,
                closeByEscapeKey,
                zIndexOffset,
                autoWidth);
        }

        public static DialogWindow ShowDialog(
            string title,
            FrameworkElement content,
            Action okAction = null,
            Action cancelAction = null,
            bool hideOkButton = false,
            bool hideCancelButton = true,
            bool focusOnCancelButton = false,
            string okText = null,
            string cancelText = null,
            bool closeByEscapeKey = false,
            int zIndexOffset = 0,
            bool autoWidth = false)
        {
            string text;
            switch (content)
            {
                case TextBlock textBlock:
                    text = textBlock.Text;
                    break;
                case FormattedTextBlock textBlock when textBlock.Content is string formattedTextContent:
                    text = formattedTextContent;
                    break;
                default:
                    text = content.ToString();
                    break;
            }

            Api.Logger.Important(
                string.Format(
                    "Dialog window shown: {0}: {1}{2}{3}",
                    title,
                    text,
                    okAction != null && !hideOkButton
                        ? $" [{okText ?? "OK"}]"
                        : string.Empty,
                    cancelAction != null
                        ? $" [{cancelText ?? "Cancel"}]"
                        : string.Empty));

            if (!hideCancelButton
                && cancelAction == null)
            {
                cancelAction = EmptyAction;
            }

            var dialogWindow = new DialogWindow()
            {
                WindowTitle = title,
                Content = content,
                OkAction = okAction,
                CancelAction = cancelAction,
                OkText = okText,
                CancelText = cancelText,
                HideOkButton = hideOkButton,
                CloseByEscapeKey = closeByEscapeKey,
                ZIndexOffset = zIndexOffset,
                FocusOnCancelButton = focusOnCancelButton,
            };

            Api.Client.UI.LayoutRootChildren.Add(dialogWindow);

            if (autoWidth)
            {
                dialogWindow.GameWindow.Width = 0;
            }

            return dialogWindow;
        }

        public static void ShowMessage(string title, string text, bool closeByEscapeKey, int zIndexOffset = 0)
        {
            Api.Logger.Important(string.Format("Dialog window shown: {0}: {1}", title, text));

            var controlWithWindow = new DialogWindow()
            {
                WindowTitle = title,
                Content = CreateTextElement(text, TextAlignment.Center),
                OkAction = () => { },
                CloseByEscapeKey = closeByEscapeKey,
                ZIndexOffset = zIndexOffset
            };

            Api.Client.UI.LayoutRootChildren.Add(controlWithWindow);
        }

        public void Close(DialogResult dialogResult)
        {
            this.window.Close(dialogResult);
        }

        // TODO: remove this hack method
        public void ForceShowCancelButton()
        {
            Grid.SetColumn(this.btnOk, 0);
            this.btnOk.Margin = new Thickness(0, 0, 5, 0);
            this.btnCancel.Visibility = Visibility.Visible;
            this.window.FocusOnControl = this.btnCancel;
        }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.window = templateRoot.GetByName<GameWindow>("Window");
            this.btnOk = templateRoot.GetByName<Button>("BtnOk");
            this.btnCancel = templateRoot.GetByName<Button>("BtnCancel");

            if (!string.IsNullOrEmpty(this.OkText))
            {
                this.btnOk.Content = this.OkText;
            }

            if (!string.IsNullOrEmpty(this.CancelText))
            {
                this.btnCancel.Content = this.CancelText;
            }

            if (this.CancelAction == null)
            {
                Grid.SetColumn(this.btnOk, 1);
                this.btnOk.Margin = new Thickness(0);
                this.btnCancel.Visibility = Visibility.Collapsed;
            }

            if (this.HideOkButton)
            {
                Grid.SetColumn(this.btnCancel, 1);
                this.btnCancel.Margin = new Thickness(0);
                this.btnOk.Visibility = Visibility.Collapsed;
            }

            //this.window.IsModal = true;
            this.window.CloseByEscapeKey = this.CloseByEscapeKey;
            this.window.FocusOnControl = this.FocusOnCancelButton && this.btnCancel.Visibility == Visibility.Visible
                                             ? this.btnCancel
                                             : this.btnOk;

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.window.WindowName = string.Format("Dialog window \"{0}\"", this.WindowTitle);
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.UpdateWindowZIndexOffset();

            this.btnOk.Click += this.BtnOkOnClick;
            this.btnCancel.Click += this.BtnCancelOnClick;
            this.window.StateChanged += this.WindowStateChangedHandler;

            // TODO: remove this hack (use class with constants to define all the common Z index offsets?)
            this.window.ZIndexOffset = 10000;
            this.window.Open();
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            this.btnOk.Click -= this.BtnOkOnClick;
            this.btnCancel.Click -= this.BtnCancelOnClick;
            this.window.StateChanged -= this.WindowStateChangedHandler;
        }

        private void BtnCancelOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.window.Close(DialogResult.Cancel);
        }

        private void BtnOkOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.window.Close(DialogResult.OK);
        }

        private void UpdateWindowZIndexOffset()
        {
            if (this.ZIndexOffset != 0
                && this.window != null)
            {
                this.window.ZIndexOffset = this.ZIndexOffset;
            }
        }

        private void WindowStateChangedHandler(GameWindow obj)
        {
            if (this.window.State != GameWindowState.Closed)
            {
                return;
            }

            var dialogResult = this.window.DialogResult;
            switch (dialogResult)
            {
                case DialogResult.OK:
                    this.OkAction?.Invoke();
                    break;

                case DialogResult.Cancel:
                    this.CancelAction?.Invoke();
                    break;
            }

            this.Closed?.Invoke(this, null);
        }
    }
}