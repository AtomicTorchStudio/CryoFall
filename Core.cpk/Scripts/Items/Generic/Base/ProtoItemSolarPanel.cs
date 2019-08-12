namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class ProtoItemSolarPanel
        : ProtoItemGeneric
          <ItemWithDurabilityPrivateState,
              EmptyPublicState,
              EmptyClientState>,
          IProtoItemSolarPanel
    {
        public abstract ushort DurabilityDecreasePerMinuteWhenInstalled { get; }

        public abstract uint DurabilityMax { get; }

        public abstract double ElectricityProductionPerSecond { get; }

        public override double GroundIconScale => 2;

        public virtual bool IsRepairable => false;

        public override ushort MaxItemsPerStack => ItemStackSize.Single;

        public abstract ITextureResource ObjectSprite { get; }

        public sealed override double ServerUpdateIntervalSeconds => 60;

        public void ClientCreateItemSlotOverlayControls(IItem item, List<Control> controls)
        {
            if (this.DurabilityMax > 0)
            {
                controls.Add(ItemSlotDurabilityOverlayControl.Create(item));
            }
        }

        public void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId)
        {
            // place a broken solar panel in the released container slot
            Server.Items.CreateItem<ItemSolarPanelBroken>(container, slotId: slotId);
        }

        public void ServerOnItemDamaged(IItem item, double damageApplied)
        {
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            ItemDurabilitySystem.ServerInitializeItem(data.PrivateState, data.IsFirstTimeInit);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var item = data.GameObject;

            if (!(item.Container?.ProtoItemsContainer is ItemsContainerGeneratorSolar))
            {
                // the panel is not installed
                return;
            }

            var worldObjectGenerator = item.Container.OwnerAsStaticObject;
            if (worldObjectGenerator == null
                || worldObjectGenerator.IsDestroyed)
            {
                return;
            }

            // The panel is installed.
            // Decrease durability proportionally to the generation rate
            // (degrade only during the daytime and only if active).
            var rate = ProtoObjectGeneratorSolar.SharedGetElectricityProductionRate(worldObjectGenerator);
            if (rate <= 0)
            {
                return;
            }

            var decrease = this.DurabilityDecreasePerMinuteWhenInstalled * (data.DeltaTime / 60.0);
            decrease *= rate;

            ItemDurabilitySystem.ServerModifyDurability(item,
                                                        delta: -(int)decrease);
        }
    }
}