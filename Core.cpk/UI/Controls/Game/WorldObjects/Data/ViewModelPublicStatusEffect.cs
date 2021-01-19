namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelPublicStatusEffect : BaseViewModel
    {
        private readonly IProtoStatusEffect protoStatusEffect;

        public ViewModelPublicStatusEffect(IProtoStatusEffect protoStatsEffect)
        {
            this.protoStatusEffect = protoStatsEffect;
        }

        public Brush BorderBrush
            => ClientStatusEffectIconColorizer.GetBrush(this.protoStatusEffect.Kind,
                                                        intensity: 1);

        public Brush ColorizedIcon
            => Api.Client.UI.GetTextureBrush(
                this.protoStatusEffect.Icon);

        public string Title => this.protoStatusEffect.Name;
    }
}