namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelItemTooltip : BaseViewModel
    {
        private readonly IItem item;

        private readonly IProtoItem protoItem;

        public ViewModelItemTooltip(IItem item, IProtoItem protoItem)
        {
            this.item = item;
            this.protoItem = protoItem;

            var controls = new List<Control>();
            this.PopulateControls(controls);

            if (controls.Count > 0)
            {
                this.InfoControls = controls;
            }
        }

        public string Description => this.protoItem.Description;

        public Brush Icon
            => !IsDesignTime
                   ? (Brush)Client.UI.GetTextureBrush(this.protoItem.Icon)
                   : Brushes.BlueViolet;

        public IReadOnlyList<Control> InfoControls { get; }

        public string Name => this.protoItem.Name;

        private void PopulateControls(List<Control> controls)
        {
            if (this.item == null)
            {
                return;
            }

            this.protoItem.ClientTooltipCreateControls(this.item, controls);
        }
    }
}