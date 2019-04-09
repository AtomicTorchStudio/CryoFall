namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans;
    using AtomicTorch.CBND.CoreMod.Systems.WateringCanRefill;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelHotbarItemWateringCanOverlayControl : BaseViewModel
    {
        private WateringCanRefillActionState currentAction;

        private IItem item;

        public ViewModelHotbarItemWateringCanOverlayControl()
        {
            if (IsDesignTime)
            {
                return;
            }

            var characterState = ClientCurrentCharacterHelper.PrivateState;
            characterState.ClientSubscribe(
                _ => _.CurrentActionState,
                s => { this.CurrentAction = s as WateringCanRefillActionState; },
                this);

            this.CurrentAction = characterState.CurrentActionState as WateringCanRefillActionState;
        }

        public IItem Item
        {
            get => this.item;
            set
            {
                if (this.item == value)
                {
                    return;
                }

                if (this.item != null)
                {
                    this.ReleaseSubscriptions();
                }

                this.item = value;

                if (this.item == null)
                {
                    return;
                }

                var itemPrivateState = this.item.GetPrivateState<ItemWateringCanPrivateState>();

                this.WaterCapacity = ((IProtoItemToolWateringCan)this.item.ProtoGameObject).WaterCapacity;
                this.WaterAmount = itemPrivateState.WaterAmount;

                itemPrivateState.ClientSubscribe(
                    _ => _.WaterAmount,
                    waterAmount => this.WaterAmount = waterAmount,
                    this);
            }
        }

        public double RefillDurationSeconds { get; private set; }

        public ushort WaterAmount { get; set; } = 10;

        public ushort WaterCapacity { get; set; } = 20;

        private WateringCanRefillActionState CurrentAction
        {
            get => this.currentAction;
            set
            {
                if (this.currentAction == value)
                {
                    return;
                }

                if (value == null
                    || value.ItemWateringCan != this.item)
                {
                    this.RefillDurationSeconds = 0;
                    return;
                }

                this.currentAction = value;
                this.RefillDurationSeconds = this.currentAction.TimeRemainsSeconds;
            }
        }
    }
}