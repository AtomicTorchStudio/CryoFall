namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Floors
{
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;

    public class FloorPatternLayer
    {
        public readonly NeighborsPattern[] ExcludesAnyCheck;

        public readonly NeighborsPattern ExcludesOnlyCheck;

        public readonly string Name;

        public readonly NeighborsPattern[] RequiresAnyCheck;

        public readonly NeighborsPattern RequiresOnlyCheck;

        public readonly FloorSourceChunk[] SourceChunks;

        public FloorPatternLayer(
            string name,
            FloorSourceChunk[] sourceChunks,
            NeighborsPattern requires,
            NeighborsPattern excludes)
        {
            this.Name = name;
            this.SourceChunks = sourceChunks;
            this.RequiresOnlyCheck = requires;
            this.ExcludesOnlyCheck = excludes;
        }

        public FloorPatternLayer(
            string name,
            FloorSourceChunk sourceChunk,
            NeighborsPattern requires,
            NeighborsPattern excludes)
            : this(name, new[] { sourceChunk }, requires, excludes)
        {
        }

        public FloorPatternLayer(
            string name,
            FloorSourceChunk sourceChunk,
            NeighborsPattern[] requiresAny,
            NeighborsPattern excludes)
        {
            this.Name = name;
            this.SourceChunks = new[] { sourceChunk };
            this.RequiresAnyCheck = requiresAny;
            this.ExcludesOnlyCheck = excludes;
        }

        public FloorPatternLayer(
            string name,
            FloorSourceChunk sourceChunk,
            NeighborsPattern[] requiresAny,
            NeighborsPattern[] excludesAny)
        {
            this.Name = name;
            this.SourceChunks = new[] { sourceChunk };
            this.RequiresAnyCheck = requiresAny;
            this.ExcludesAnyCheck = excludesAny;
        }

        public FloorPatternLayer(
            string name,
            FloorSourceChunk sourceChunk,
            NeighborsPattern requires,
            NeighborsPattern[] excludesAny)
        {
            this.Name = name;
            this.SourceChunks = new[] { sourceChunk };
            this.RequiresOnlyCheck = requires;
            this.ExcludesAnyCheck = excludesAny;
        }

        public bool IsPass(NeighborsPattern variant)
        {
            if (this.IsExcluded(variant))
            {
                return false;
            }

            if (this.RequiresAnyCheck != null)
            {
                foreach (var pattern in this.RequiresAnyCheck)
                {
                    if ((pattern & variant) == pattern)
                    {
                        // pass
                        return true;
                    }
                }

                // not pass
                return false;
            }

            return (this.RequiresOnlyCheck & variant) == this.RequiresOnlyCheck;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}",
                                 nameof(this.ExcludesOnlyCheck),
                                 this.ExcludesOnlyCheck,
                                 nameof(this.Name),
                                 this.Name,
                                 nameof(this.RequiresOnlyCheck),
                                 this.RequiresOnlyCheck,
                                 nameof(this.RequiresAnyCheck),
                                 this.RequiresAnyCheck,
                                 nameof(this.SourceChunks),
                                 this.SourceChunks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsExcluded(NeighborsPattern variant)
        {
            if (this.ExcludesAnyCheck != null)
            {
                foreach (var pattern in this.ExcludesAnyCheck)
                {
                    if ((pattern & variant) == NeighborsPattern.None)
                    {
                        // not excluded pattern found
                        return false;
                    }
                }

                // excluded
                return true;
            }

            if (this.ExcludesOnlyCheck == NeighborsPattern.None
                || (this.ExcludesOnlyCheck & variant) == NeighborsPattern.None)
            {
                // not excluded
                return false;
            }

            // excluded
            return true;
        }
    }
}