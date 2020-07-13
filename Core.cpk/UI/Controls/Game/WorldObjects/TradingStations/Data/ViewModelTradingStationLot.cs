namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.Systems.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelTradingStationLot : BaseViewModel
    {
        private readonly Action<TradingStationLot, ViewModelTradingStationLotEditor> callbackSaveHandler;

        private readonly TradingStationLot lot;

        public ViewModelTradingStationLot(
            TradingStationLot lot,
            Action<TradingStationLot, ViewModelTradingStationLotEditor> callbackSaveHandler = null)
        {
            this.lot = lot;
            this.callbackSaveHandler = callbackSaveHandler;

            this.lot.ClientSubscribe(_ => _.ProtoItem,
                                     _ =>
                                     {
                                         this.NotifyPropertyChanged(nameof(this.ProtoItem));
                                         this.NotifyPropertyChanged(nameof(this.Icon));
                                     },
                                     this);

            this.lot.ClientSubscribe(_ => _.ItemOnSale,
                                     _ =>
                                     {
                                         this.NotifyPropertyChanged(nameof(this.ItemOnSaleInstance));
                                         this.RefreshOverlayControls();
                                     },
                                     this);

            this.RefreshOverlayControls();

            this.lot.ClientSubscribe(_ => _.LotQuantity,
                                     _ => this.NotifyPropertyChanged(nameof(this.LotQuantity)),
                                     this);

            this.lot.ClientSubscribe(_ => _.PriceCoinPenny,
                                     _ => this.NotifyPropertyChanged(nameof(this.PriceCoinPenny)),
                                     this);

            this.lot.ClientSubscribe(_ => _.PriceCoinShiny,
                                     _ => this.NotifyPropertyChanged(nameof(this.PriceCoinShiny)),
                                     this);

            this.lot.ClientSubscribe(_ => _.State,
                                     _ =>
                                     {
                                         this.NotifyPropertyChanged(nameof(this.IsAvailable));
                                         this.NotifyPropertyChanged(nameof(this.IsEnabled));
                                         this.NotifyPropertyChanged(nameof(this.ProblemText));
                                     },
                                     this);
        }

        public BaseCommand CommandBuy => new ActionCommand(this.ExecuteCommandBuy);

        public BaseCommand CommandConfigure => new ActionCommand(this.ExecuteCommandConfigure);

        public BaseCommand CommandSell => new ActionCommand(this.ExecuteCommandSell);

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.lot.ProtoItem?.Icon);

        public Brush IconCoinPenny => UITradingIcons.LazyIconCoinPenny.Value;

        public Brush IconCoinShiny => UITradingIcons.LazyIconCoinShiny.Value;

        public bool IsAvailable => this.lot.State == TradingStationLotState.Available;

        public bool IsEnabled => this.lot.State != TradingStationLotState.Disabled;

        public IItem ItemOnSaleInstance => this.lot.ItemOnSale;

        public ushort LotQuantity => this.lot.LotQuantity;

        public IReadOnlyList<Control> OverlayControls { get; private set; }

        public uint PriceCoinPenny => this.lot.PriceCoinPenny;

        public uint PriceCoinShiny => this.lot.PriceCoinShiny;

        public string ProblemText => this.GetProblemText();

        public IProtoItem ProtoItem => this.lot.ProtoItem;

        protected override void DisposeViewModel()
        {
            WindowTradingStationLotEditor.CloseWindowIfOpened();
            base.DisposeViewModel();
        }

        private void ExecuteCommandBuy()
        {
            TradingStationsSystem.ClientRequestExecuteTrade(
                (IStaticWorldObject)this.lot.GameObject,
                this.lot,
                isPlayerBuying: true);
        }

        private void ExecuteCommandConfigure()
        {
            var window = new WindowTradingStationLotEditor(this.lot);
            window.EventWindowClosing += WindowClosingHandler;
            Api.Client.UI.LayoutRootChildren.Add(window);

            void WindowClosingHandler()
            {
                window.EventWindowClosing -= WindowClosingHandler;
                if (window.DialogResult == DialogResult.OK)
                {
                    this.callbackSaveHandler(this.lot, window.ViewModel);
                }
            }
        }

        private void ExecuteCommandSell()
        {
            TradingStationsSystem.ClientRequestExecuteTrade(
                (IStaticWorldObject)this.lot.GameObject,
                this.lot,
                isPlayerBuying: false);
        }

        private string GetProblemText()
        {
            return this.lot.State.GetDescription();
        }

        private void RefreshOverlayControls()
        {
            var item = this.lot.ItemOnSale;
            if (item?.ProtoItem is IProtoItemWithSlotOverlay protoItemWithSlotOverlay)
            {
                var controls = new List<Control>();
                protoItemWithSlotOverlay.ClientCreateItemSlotOverlayControls(item, controls);
                this.OverlayControls = controls;
            }
            else
            {
                this.OverlayControls = null;
            }
        }
    }
}