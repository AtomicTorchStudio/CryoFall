namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolGenerator
{
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.GameApi.Resources;

    public class EditorToolGeneratorItem : BaseEditorToolItem
    {
        public readonly BaseEditorToolGeneratorAlgorithm Generator;

        public EditorToolGeneratorItem(BaseEditorToolGeneratorAlgorithm generator) : base(generator.Name, generator.Id)
        {
            this.Generator = generator;
        }

        public override ITextureResource Icon => this.Generator.Icon;
    }
}