namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public abstract class ProtoItemConsumable : ProtoItemGeneric, IProtoItemUsableFromContainer
    {
        public IReadOnlyList<EffectAction> Effects { get; private set; }

        public virtual string ItemUseCaption => ItemUseCaptions.Use;

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            var item = data.Item;
            var character = Client.Characters.CurrentPlayerCharacter;
            var stats = PlayerCharacter.GetPublicState(character).CurrentStatsExtended;

            if (!this.SharedCanUse(character, stats))
            {
                return false;
            }

            this.CallServer(_ => _.ServerRemote_Use(item));
            return true;
        }

        protected override void ClientTooltipCreateControlsInternal(IItem item, List<UIElement> controls)
        {
            base.ClientTooltipCreateControlsInternal(item, controls);

            if (this.Effects.Count > 0)
            {
                controls.Add(ItemTooltipInfoEffectActionsControl.Create(this.Effects));
            }
        }

        protected virtual void PrepareEffects(EffectActionsList effects)
        {
        }

        protected sealed override void PrepareProtoItem()
        {
            base.PrepareProtoItem();

            var effects = new EffectActionsList();

            this.PrepareEffects(effects);
            this.Effects = effects.ToReadOnly();

            this.PrepareProtoItemConsumable();
        }

        protected virtual void PrepareProtoItemConsumable()
        {
        }

        protected abstract void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats);

        protected virtual bool SharedCanUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            return true;
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered,
                            timeInterval: 0.2,
                            clientMaxSendQueueSize: 20)]
        private void ServerRemote_Use(IItem item)
        {
            var character = ServerRemoteContext.Character;
            this.ServerValidateItemForRemoteCall(item, character);

            var publicState = PlayerCharacter.GetPublicState(character);
            if (publicState.IsDead
                || PlayerCharacter.GetPrivateState(character).IsDespawned)
            {
                return;
            }

            var stats = publicState.CurrentStatsExtended;
            if (!this.SharedCanUse(character, stats))
            {
                return;
            }

            foreach (var effect in this.Effects)
            {
                effect.Execute(new EffectActionContext(character));
            }

            this.ServerOnUse(character, stats);

            Logger.Important(character + " has used " + item);

            this.ServerNotifyItemUsed(character, item);
            // decrease item count
            Server.Items.SetCount(item, (ushort)(item.Count - 1));
        }
    }
}