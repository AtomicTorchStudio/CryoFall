namespace AtomicTorch.CBND.CoreMod.Items.DataLogs.Base
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemDataLog : IProtoItem, IProtoItemUsableFromContainer
    {
        DataLogLocation FromLocation { get; }

        DataLogPerson FromPerson { get; }

        string Text { get; }

        DataLogLocation ToLocation { get; }

        DataLogPerson ToPerson { get; }
    }
}