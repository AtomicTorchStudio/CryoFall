namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class HintsList
    {
        private readonly List<Func<string>> list = new();

        public HintsList Add(string hint)
        {
            this.list.Add(() => hint);
            return this;
        }

        public HintsList Add(Func<string> hint)
        {
            this.list.Add(hint);
            return this;
        }

        public override string ToString()
        {
            if (this.list.Count == 0)
            {
                return null;
            }

            var result = this.list
                             .Select(func => func().ToString())
                             .GetJoinedString("[*]")
                             .ToString()
                             .Replace("[*][*]", string.Empty);

            if (!result.StartsWith("[*]", StringComparison.Ordinal))
            {
                result = "[*]" + result;
            }

            return result;
        }
    }
}