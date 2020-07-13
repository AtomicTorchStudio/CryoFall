namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;

    public class ViewModelActionStatusEffect : ViewModelPublicStatusEffect
    {
        private readonly EffectAction effectAction;

        public ViewModelActionStatusEffect(EffectAction effectAction)
            : base(effectAction.ProtoStatusEffect)
        {
            this.effectAction = effectAction;
        }

        public string IntensityPercentText
        {
            get
            {
                var result = (int)(this.effectAction.Intensity * 100);
                var text = result.ToString() + "%";
                return result > 0
                           ? "+" + text
                           : text;
            }
        }
    }
}