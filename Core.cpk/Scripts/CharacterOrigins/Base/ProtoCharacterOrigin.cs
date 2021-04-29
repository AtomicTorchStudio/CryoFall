namespace AtomicTorch.CBND.CoreMod.CharacterOrigins
{
    using System;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class ProtoCharacterOrigin : ProtoEntity
    {
        private TextureResource cachedIcon;

        protected ProtoCharacterOrigin()
        {
            var name = this.GetType().Name;
            if (!name.StartsWith("CharacterOrigin", StringComparison.Ordinal))
            {
                throw new Exception("Origin class name must start with \"CharacterOrigin\": " + this.GetType().Name);
            }

            this.ShortId = name.Substring("CharacterOrigin".Length);
        }

        public abstract string Description { get; }

        public IReadOnlyStatsDictionary Effects { get; private set; }

        public ITextureResource Icon
            => this.cachedIcon ??= new TextureResource("CharacterOrigins/" + this.ShortId);

        public override string ShortId { get; }

        public void SharedReinitializeDefaultEffects()
        {
            var effects = new Effects();
            this.FillDefaultEffects(effects);
            this.Effects = effects.ToReadOnly();
        }

        protected abstract void FillDefaultEffects(Effects effects);

        protected override void PrepareProto()
        {
            this.SharedReinitializeDefaultEffects();
            this.PrepareProtoCharacterOrigin();
        }

        protected virtual void PrepareProtoCharacterOrigin()
        {
        }
    }
}