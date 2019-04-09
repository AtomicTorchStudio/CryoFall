namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.DataLogs.Data
{
    using AtomicTorch.CBND.CoreMod.Items.DataLogs.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowDataLog : BaseViewModel
    {
        private readonly IProtoItemDataLog proto;

        public ViewModelWindowDataLog(IItem itemDataLog)
        {
            this.proto = (IProtoItemDataLog)itemDataLog.ProtoItem;
        }

        public string FromLocation => this.proto.FromLocation.GetDescription();

        public string FromPerson => this.proto.FromPerson.GetDescription();

        public string Text => this.proto.Text;

        public string ToLocation => this.proto.ToLocation.GetDescription();

        public string ToPerson => this.proto.ToPerson.GetDescription();
    }
}