namespace AtomicTorch.CBND.CoreMod.Items.DataLogs.Base
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.DataLogs;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class ProtoItemDataLog
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItem
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemDataLog
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected ProtoItemDataLog()
        {
            this.Icon = new TextureResource(
                "Items/DataLogs/" + this.GetType().Name.Replace("ItemDataLog", string.Empty));
        }

        public abstract DataLogLocation FromLocation { get; }

        public abstract DataLogPerson FromPerson { get; }

        public override ITextureResource Icon { get; }

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

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemDataLog;
        }
    }

    public abstract class ProtoItemDataLog
        : ProtoItemDataLog<EmptyPrivateState, EmptyPublicState, EmptyClientState>
    {
    }
}