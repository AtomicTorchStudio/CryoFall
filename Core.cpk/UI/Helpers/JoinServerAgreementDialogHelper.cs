namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using AtomicTorch.CBND.CoreMod.ClientOptions.General;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    internal static class JoinServerAgreementDialogHelper
    {
        public static Task Display()
        {
            // we don't need this feature anymore but we might want to enable it again later
            return Task.CompletedTask;

            if (GeneralOptionDeveloperMode.IsEnabled)
            {
                return Task.CompletedTask;
            }

            const string text =
                @"We need your help!

Currently CryoFall is completely free during the alpha stage and we want to keep it that way for as long as we can! We want to allow more people to try the game, participate in the development process and engage with the community without any barriers.

But we cannot do that alone... Please consider sharing CryoFall with your friends (they too can play for free!), sharing on social media, recording Youtube let's plays and reviews or streaming on Twitch, post it to Reddit or Facebook, etc.

We really need your help to create enough buzz around the game if we ever want to see CryoFall succeed.";

            var stackpanel = new StackPanel();
            stackpanel.Children.Add(new TextBlock()
            {
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap
            });

            var checkbox = new CheckBox()
            {
                Content = "I will do what I can!",
                Margin = new Thickness(0, 11, 0, 7),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            stackpanel.Children.Add(checkbox);

            var button = new Button()
            {
                Content = "Confirm",
                Margin = new Thickness(0, 8, 0, 1)
            };
            stackpanel.Children.Add(button);

            button.SetBinding(UIElement.IsEnabledProperty,
                              new Binding()
                              {
                                  Path = new PropertyPath(ToggleButton.IsCheckedProperty.Name),
                                  Source = checkbox
                              });

            var taskCompletionSource = new TaskCompletionSource<bool>();

            var dialog = DialogWindow.ShowDialog(
                "Important!",
                content: stackpanel,
                hideOkButton: true,
                okAction: () => { taskCompletionSource.SetResult(true); });

            button.Click += (s, e) => dialog.Close(DialogResult.OK);

            return taskCompletionSource.Task;
        }
    }
}