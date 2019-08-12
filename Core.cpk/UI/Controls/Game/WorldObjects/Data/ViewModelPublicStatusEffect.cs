namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelPublicStatusEffect : BaseViewModel
    {
        private const byte BackgroundOpacity = 0x66;

        public static readonly Brush BrushBuff
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0x00, 0xCC, 0x00));

        public static readonly Brush BrushDebuff
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0xFF, 0x11, 0x11));

        public static readonly Brush BrushNeutral
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0xBB, 0xBB, 0xBB));

        private readonly IProtoStatusEffect protoStatusEffect;

        public ViewModelPublicStatusEffect(IProtoStatusEffect protoStatsEffect)
        {
            this.protoStatusEffect = protoStatsEffect;
        }

        public Brush BackgroundBrush => GetBrush(this.protoStatusEffect.Kind);

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.protoStatusEffect.Icon);

        public static Brush GetBrush(StatusEffectKind kind)
        {
            switch (kind)
            {
                case StatusEffectKind.Buff:
                    return BrushBuff;

                case StatusEffectKind.Debuff:
                    return BrushDebuff;

                case StatusEffectKind.Neutral:
                    return BrushNeutral;
            }

            throw new Exception("Impossible");
        }
    }
}