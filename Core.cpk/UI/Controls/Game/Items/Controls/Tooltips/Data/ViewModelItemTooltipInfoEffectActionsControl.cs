namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;

    public class ViewModelItemTooltipInfoEffectActionsControl : BaseViewModel
    {
        public ViewModelItemTooltipInfoEffectActionsControl(IReadOnlyList<EffectAction> effectActions)
        {
            this.EffectsToAdd = effectActions.Where(a => a.Intensity > 0
                                                         && !a.IsHidden
                                                         && a.ProtoStatusEffect.IsPublic)
                                             .Select(a => new ViewModelActionStatusEffect(a))
                                             .ToArray();

            this.EffectsToRemove = effectActions.Where(a => a.Intensity < 0
                                                            && !a.IsHidden
                                                            && a.ProtoStatusEffect.IsPublic)
                                                .Select(a => new ViewModelActionStatusEffect(a))
                                                .ToArray();
        }

        public ViewModelPublicStatusEffect[] EffectsToAdd { get; }

        public ViewModelPublicStatusEffect[] EffectsToRemove { get; }

        public bool HasEffectsToAdd => this.EffectsToAdd.Length > 0;

        public bool HasEffectsToAddAndRemove => this.HasEffectsToAdd
                                                && this.HasEffectsToRemove;

        public bool HasEffectsToRemove => this.EffectsToRemove.Length > 0;
    }
}