namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HotbarItemPragmiumSensorOverlayControl : BaseUserControl
    {
        private readonly IItem item;

        private Grid grid;

        private Storyboard storyboardAnimationSignalPing;

        private Storyboard storyboardAnimationSignalPong;

        public HotbarItemPragmiumSensorOverlayControl()
        {
        }

        public HotbarItemPragmiumSensorOverlayControl(IItem item)
        {
            this.item = item;
        }

        protected override void InitControl()
        {
            this.grid = this.GetByName<Grid>("Grid");

            // wrap the content into a hotbar extension control
            this.Content = null;
            this.Content = new HotbarItemSlotExtensionControl()
            {
                SlotContent = this.grid
            };

            this.storyboardAnimationSignalPing = this.grid.GetResource<Storyboard>("StoryboardAnimationSignalPing");
            this.storyboardAnimationSignalPong = this.grid.GetResource<Storyboard>("StoryboardAnimationSignalPong");

            // we cannot seek Storyboard in NoesisGUI yet https://www.noesisengine.com/bugs/view.php?id=1484
            // apply a workaround
            this.grid.Visibility = Visibility.Hidden;
            this.storyboardAnimationSignalPing.Begin(this.grid);
            this.storyboardAnimationSignalPong.Begin(this.grid);

            ClientTimersSystem.AddAction(
                Math.Max(this.storyboardAnimationSignalPing.Duration.TimeSpan.TotalSeconds,
                         this.storyboardAnimationSignalPong.Duration.TimeSpan.TotalSeconds),
                () => this.grid.Visibility = Visibility.Visible);
        }

        protected override void OnLoaded()
        {
            ProtoItemPragmiumSensor.ServerSignalReceived += this.SignalReceivedHandler;
            ClientHotbarSelectedItemManager.SelectedItemChanged += this.HotbarSelectedItemChangedHandler;
            this.RefreshVisibility();
        }

        protected override void OnUnloaded()
        {
            ProtoItemPragmiumSensor.ServerSignalReceived -= this.SignalReceivedHandler;
            ClientHotbarSelectedItemManager.SelectedItemChanged -= this.HotbarSelectedItemChangedHandler;
        }

        private void HotbarSelectedItemChangedHandler(IItem item)
        {
            this.RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            this.Visibility = ReferenceEquals(this.item, ClientHotbarSelectedItemManager.SelectedItem)
                                  ? Visibility.Visible
                                  : Visibility.Hidden;
        }

        private void SignalReceivedHandler(IItem itemSignalSource, PragmiumSensorSignalKind signalKind)
        {
            if (!ReferenceEquals(this.item, itemSignalSource))
            {
                // received a signal for different item
                return;
            }

            var storyboard = signalKind switch
            {
                PragmiumSensorSignalKind.Ping => this.storyboardAnimationSignalPing,
                PragmiumSensorSignalKind.Pong => this.storyboardAnimationSignalPong,
                _ => throw new ArgumentOutOfRangeException(nameof(signalKind), signalKind, null)
            };

            storyboard.Begin(this.grid);
        }
    }
}