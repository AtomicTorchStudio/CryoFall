namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Floors
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;

    public class FloorChunkPreset
    {
        public readonly FloorSourceChunk[] Layers;

        public readonly int LayersHashCode;

        public readonly NeighborsPattern Pattern;

        public readonly byte TargetColumn;

        public readonly byte TargetRow;

        public IEnumerable<FloorSourceChunk> LinkedLayersFromOtherChunk;

        public FloorChunkPreset(
            NeighborsPattern pattern,
            byte targetColumn,
            byte targetRow,
            FloorSourceChunk[] layers,
            int layersHashCode,
            IEnumerable<FloorSourceChunk> linkedLayersFromOtherChunk)
        {
            this.Pattern = pattern;
            this.Layers = layers;
            this.LayersHashCode = layersHashCode;
            this.LinkedLayersFromOtherChunk = linkedLayersFromOtherChunk;
            this.TargetColumn = targetColumn;
            this.TargetRow = targetRow;
        }
    }
}