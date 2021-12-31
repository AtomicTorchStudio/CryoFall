namespace AtomicTorch.CBND.CoreMod.Items.DataLogs.Base
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.DataLogs;
    using AtomicTorch.CBND.GameApi.Data.State;

    public abstract class ProtoItemDataLog
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItem
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemDataLog
        where TPrivateState : ItemPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public abstract DataLogLocation FromLocation { get; }

        public abstract DataLogPerson FromPerson { get; }

        public string ItemUseCaption => ItemUseCaptions.Read;

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public abstract string Text { get; }

        public abstract DataLogLocation ToLocation { get; }

        public abstract DataLogPerson ToPerson { get; }

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            WindowDataLog.Open(data.Item);
            return true;
        }

        protected override string GenerateIconPath()
        {
            return "Items/DataLogs/" + this.GetType().Name.Replace("ItemDataLog", string.Empty);
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemDataLog;
        }
    }

    public abstract class ProtoItemDataLog
        : ProtoItemDataLog
            <ItemPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}