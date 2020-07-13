namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipInfoEffectActionsControl : BaseUserControl
    {
        private readonly IReadOnlyList<EffectAction> effectActions;

        private ViewModelItemTooltipInfoEffectActionsControl viewModel;

        private ItemTooltipInfoEffectActionsControl(IReadOnlyList<EffectAction> effectActions)
        {
            this.effectActions = effectActions;
        }

        public static UIElement Create(IReadOnlyList<EffectAction> effectActions)
        {
            return new ItemTooltipInfoEffectActionsControl(effectActions);
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelItemTooltipInfoEffectActionsControl(this.effectActions);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}