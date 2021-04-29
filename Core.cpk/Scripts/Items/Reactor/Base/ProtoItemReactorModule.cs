namespace AtomicTorch.CBND.CoreMod.Items.Reactor
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public abstract class ProtoItemReactorModule : ProtoItemWithDurability
    {
        public sealed override uint DurabilityMax => uint.MaxValue;

        public virtual double EfficiencyModifierPercents => 0;

        public virtual double FuelLifetimeModifierPercents => 0;

        public override bool IsRepairable => true;

        public virtual double LifetimeDuration => 10 * 24 * 60 * 60;

        public virtual double PsiEmissionModifierValue => 0;

        public virtual double StartupShutdownTimeModifierPercents => 0;

        public override void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId)
        {
            // place a broken part to the released container slot
            Server.Items.CreateItem<ItemReactorBrokenModule>(container, slotId: slotId);
        }

        protected override void ClientTooltipCreateControlsInternal(IItem item, List<UIElement> controls)
        {
            AddPercent(CoreStrings.WindowGeneratorPragmium_ReactorStats_FuelLifetime,
                       this.FuelLifetimeModifierPercents);

            AddPercent(CoreStrings.WindowGeneratorPragmium_ReactorStats_EfficiencyMax,
                       this.EfficiencyModifierPercents);

            AddPercent(CoreStrings.WindowGeneratorPragmium_ReactorStats_StartupShutdownTime,
                       this.StartupShutdownTimeModifierPercents);

            AddValue(CoreStrings.WindowGeneratorPragmium_ReactorStats_PsiEmissionLevelMax,
                     this.PsiEmissionModifierValue);

            // padding
            controls.Add(new Control() { Height = 8 });

            base.ClientTooltipCreateControlsInternal(item, controls);

            void AddValue(string title, double value)
            {
                if (value == 0)
                {
                    return;
                }

                var sign = value > 0 ? "+" : default;
                controls.Add(
                    ItemPropertiesTooltipHelper.Create(
                        title,
                        sign + value.ToString(CultureInfo.InvariantCulture)));
            }

            void AddPercent(string title, double value)
            {
                if (value == 0)
                {
                    return;
                }

                var sign = value > 0 ? "+" : default;
                controls.Add(
                    ItemPropertiesTooltipHelper.Create(
                        title,
                        sign + value.ToString(CultureInfo.InvariantCulture) + "%"));
            }
        }

        protected override string GenerateIconPath()
        {
            return "Items/Reactor/" + this.GetType().Name;
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.ReactorItem);
        }
    }
}