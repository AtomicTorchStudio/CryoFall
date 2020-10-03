namespace AtomicTorch.CBND.CoreMod.Items.DataLogs.Base
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;

    // not localized as not used in the game yet
    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public abstract class ProtoItemDataLogUnknown
        : ProtoItem<EmptyPrivateState, EmptyPublicState, EmptyClientState>, IProtoItemUsableFromContainer
    {
        private static bool isAwaitingResponse;

        private IReadOnlyList<IProtoItemDataLog> dataLogCandidates;

        protected ProtoItemDataLogUnknown()
        {
            //this.Icon = new TextureResource("Items/DataLogs/Unknown.png");
        }

        public override ITextureResource Icon => null;

        public string ItemUseCaption => ItemUseCaptions.Decrypt;

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Data Log (encrypted)";

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            if (isAwaitingResponse)
            {
                return false;
            }

            this.ClientSendUseItemRequest(data);
            return true;
        }

        protected abstract void PrepareDataLogCandidates(ItemDataLogList candidates);

        protected sealed override void PrepareProtoItem()
        {
            if (IsClient)
            {
                return;
            }

            var candidates = new ItemDataLogList();
            this.PrepareDataLogCandidates(candidates);
            this.dataLogCandidates = candidates.Freeze();
            if (this.dataLogCandidates.Count == 0)
            {
                throw new Exception(
                    $"No data logs provided during {nameof(this.PrepareDataLogCandidates)} method call for {this}");
            }
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return base.PrepareSoundPresetItem().Clone().Clear(ItemSound.Use);
        }

        protected virtual IProtoItemDataLog ServerGetDataLogProtoToSpawn()
        {
            return this.dataLogCandidates.TakeByRandom();
        }

        private async void ClientSendUseItemRequest(ClientItemData data)
        {
            isAwaitingResponse = true;

            if (Client.CurrentGame.PingGameSeconds >= 0.3)
            {
                // too big ping (>=300 ms) - show notification for immediate response
                NotificationSystem.ClientShowNotification(
                    "Decoding Data Log",
                    "...",
                    NotificationColor.Good,
                    this.Icon,
                    playSound: false);
            }

            try
            {
                var actualDataLogItem = await this.CallServer(_ => _.ServerRemote_UseItem(data.Item));
                if (actualDataLogItem is not null)
                {
                    NotificationSystem.ClientShowNotification(
                        "Decoding Data Log",
                        "Successfully decoded:"
                        + Environment.NewLine
                        + actualDataLogItem.ProtoItem.Name,
                        NotificationColor.Good,
                        actualDataLogItem.ProtoItem.Icon,
                        playSound: false);
                }
                else
                {
                    NotificationSystem.ClientShowNotification(
                        "Decoding Data Log",
                        "Problems with decoding the data log.",
                        NotificationColor.Bad,
                        this.Icon,
                        playSound: false);
                }
            }
            finally
            {
                isAwaitingResponse = false;
            }
        }

        private IItem ServerRemote_UseItem(IItem item)
        {
            var character = ServerRemoteContext.Character;
            this.ServerValidateItemForRemoteCall(item, character);

            var protoDataLog = this.ServerGetDataLogProtoToSpawn();
            if (protoDataLog is null)
            {
                throw new Exception(
                    $"Prototype for data log is not returned by {nameof(this.ServerGetDataLogProtoToSpawn)}() method");
            }

            Logger.Important($"{character} has used data log {item} and received {protoDataLog}");

            var slotId = item.ContainerSlotId;
            var container = item.Container;
            Server.Items.SetCount(item, item.Count - 1);

            CreateItemResult result;
            if (item.IsDestroyed)
            {
                // try to spawn right there
                result = Server.Items.CreateItem(protoDataLog, container, slotId: slotId);
            }
            else
            {
                // try to spawn simply in player
                var tempDropItemsList = new DropItemsList().Add(protoDataLog).AsReadOnly();
                result = ServerDroplistHelper.TryDropToCharacter(
                    tempDropItemsList,
                    character,
                    sendNoFreeSpaceNotification: true,
                    probabilityMultiplier: 1,
                    context: new DropItemContext(character, null));
            }

            if (!result.IsEverythingCreated)
            {
                // rollback, try spawn data log back
                if (item.IsDestroyed)
                {
                    // this item was destroyed - need to spawn it back
                    Server.Items.CreateItem(this, container, slotId: slotId);
                }
                else
                {
                    // this item was not destroyed - restore count
                    Server.Items.SetCount(item, item.Count + 1);
                }

                return null;
            }

            return result.ItemAmounts.FirstOrDefault().Key;
        }

        protected class ItemDataLogList
        {
            private readonly HashSet<IProtoItemDataLog> hashset = new HashSet<IProtoItemDataLog>();

            public void Add<TProtoItemDataLog>()
                where TProtoItemDataLog : IProtoItemDataLog, new()
            {
                this.hashset.Add(Api.GetProtoEntity<TProtoItemDataLog>());
            }

            public void AddAllWhere(Func<IProtoItemDataLog, bool> func)
            {
                var candidates = Api.FindProtoEntities<IProtoItemDataLog>().Where(func);
                this.hashset.AddRange(candidates);
            }

            public IReadOnlyList<IProtoItemDataLog> Freeze()
            {
                return this.hashset.ToList();
            }
        }
    }
}