namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelItemTooltip : BaseViewModel
    {
        private readonly IItem item;

        private readonly IProtoItem protoItem;

        private IProtoItem baseProtoItem;

        public ViewModelItemTooltip(IItem item, IProtoItem protoItem)
        {
            this.item = item;
            this.protoItem = protoItem;

            var controls = new List<UIElement>();
            this.PopulateControls(controls);

            if (controls.Count > 0)
            {
                this.InfoControls = controls;
            }

            this.baseProtoItem = ((IProtoItemWithSkinData)protoItem).BaseProtoItem;
        }

        public string BaseProtoItemName => this.baseProtoItem?.Name;

        public string CreatedByPlayerName
        {
            get
            {
                if (this.item is null
                    || !this.item.ClientHasPrivateState)
                {
                    return null;
                }

                return this.item.GetPrivateState<ItemPrivateState>().CreatedByPlayerName;
            }
        }

        public string Description => this.protoItem.Description;

        public Brush Icon
        {
            get
            {
                if (IsDesignTime)
                {
                    return Brushes.BlueViolet;
                }

                return Client.UI.GetTextureBrush(
                    this.item is null
                        ? this.protoItem.Icon
                        : this.protoItem.ClientGetIcon(this.item));
            }
        }

        public IReadOnlyList<UIElement> InfoControls { get; }

        public string Name => this.protoItem.Name;

        public string NameWithBaseProtoItemName
        {
            get
            {
                if (this.baseProtoItem is null)
                {
                    return this.Name;
                }

                return string.Format("{0} ({1})", this.Name, this.baseProtoItem.Name);
            }
        }

        private void PopulateControls(List<UIElement> controls)
        {
            this.protoItem.ClientTooltipCreateControls(this.item, controls);
        }
    }
}