namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public partial class WindowSetMemberRole : BaseUserControlWithWindow
    {
        public static readonly DependencyProperty EmblemProperty =
            DependencyProperty.Register("Emblem",
                                        typeof(Brush),
                                        typeof(WindowSetMemberRole),
                                        new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty MemberNameProperty
            = DependencyProperty.Register("MemberName",
                                          typeof(string),
                                          typeof(WindowSetMemberRole),
                                          new PropertyMetadata(default(string)));

        private Button buttonCancel;

        private Button buttonSave;

        private StackPanel stackPanelRoleRadioButtonsGroup;

        public Brush Emblem
        {
            get => (Brush)this.GetValue(EmblemProperty);
            set => this.SetValue(EmblemProperty, value);
        }

        public string MemberName
        {
            get => (string)this.GetValue(MemberNameProperty);
            set => this.SetValue(MemberNameProperty, value);
        }

        public static void Open(string memberName)
        {
            var window = new WindowSetMemberRole();
            window.MemberName = memberName;
            Api.Client.UI.LayoutRootChildren.Add(window);
            //FactionSystem.ClientOfficerSetMemberRole(memberName, role)
        }

        protected override void InitControlWithWindow()
        {
            this.buttonSave = this.GetByName<Button>("ButtonSave");
            this.buttonCancel = this.GetByName<Button>("ButtonCancel");

            this.Emblem = Api.Client.UI.GetTextureBrush(
                ClientFactionEmblemTextureComposer.GetEmblemTexture(
                    FactionSystem.ClientCurrentFaction,
                    useCache: true));

            this.stackPanelRoleRadioButtonsGroup = this.GetByName<StackPanel>("RoleRadioButtonsGroup");
            var children = this.stackPanelRoleRadioButtonsGroup.Children;
            var roles = EnumExtensions.GetValues<FactionMemberRole>().ExceptOne(FactionMemberRole.Leader);

            // ReSharper disable once PossibleInvalidOperationException
            var currentMemberRole = FactionSystem
                                    .SharedGetMemberEntry(this.MemberName, FactionSystem.ClientCurrentFaction)
                                    .Value
                                    .Role;

            foreach (var role in roles)
            {
                children.Add(
                    new RadioButton()
                    {
                        Content = new TextBlock() { Text = FactionSystem.ClientGetRoleTitle(role) },
                        GroupName = "RoleRadioButtonsGroup",
                        Tag = role,
                        IsChecked = role == currentMemberRole,
                        Margin = new Thickness(0, 5, 0, 5)
                    });
            }
        }

        protected override void OnLoaded()
        {
            this.buttonSave.Click += this.ButtonSaveClickHandler;
            this.buttonCancel.Click += this.ButtonCancelClickHandler;
        }

        protected override void OnUnloaded()
        {
            this.buttonSave.Click -= this.ButtonSaveClickHandler;
            this.buttonCancel.Click += this.ButtonCancelClickHandler;
        }

        private void ButtonCancelClickHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Window.Close(DialogResult.Cancel);
        }

        private void ButtonSaveClickHandler(object sender, RoutedEventArgs e)
        {
            this.Window.Close(DialogResult.OK);

            foreach (RadioButton radiobutton in this.stackPanelRoleRadioButtonsGroup.Children)
            {
                if (radiobutton.IsChecked != true)
                {
                    continue;
                }

                FactionSystem.ClientOfficerSetMemberRole(this.MemberName,
                                                         (FactionMemberRole)radiobutton.Tag);
            }
        }
    }
}