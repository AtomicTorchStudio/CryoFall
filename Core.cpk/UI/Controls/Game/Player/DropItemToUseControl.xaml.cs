namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class DropItemToUseControl : BaseUserControl
    {
        public static readonly DependencyProperty CaptionProperty
            = DependencyProperty.Register(nameof(Caption),
                                          typeof(string),
                                          typeof(DropItemToUseControl),
                                          new PropertyMetadata("Use"));

        public static readonly DependencyProperty CommandUseItemProperty
            = DependencyProperty.Register(nameof(CommandUseItem),
                                          typeof(BaseCommand),
                                          typeof(DropItemToUseControl),
                                          new PropertyMetadata(default(BaseCommand)));

        public string Caption
        {
            get => (string)this.GetValue(CaptionProperty);
            set => this.SetValue(CaptionProperty, value);
        }

        public BaseCommand CommandUseItem
        {
            get => (BaseCommand)this.GetValue(CommandUseItemProperty);
            set => this.SetValue(CommandUseItemProperty, value);
        }

        protected override void OnLoaded()
        {
            this.MouseUp += this.MouseUpHandler;
        }

        protected override void OnUnloaded()
        {
            this.MouseUp -= this.MouseUpHandler;
        }

        private void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            var item = ClientItemInHandDisplayManager.HandContainer.GetItemAtSlot(0);
            if (item is not null)
            {
                this.CommandUseItem.Execute(item);
            }
        }
    }
}