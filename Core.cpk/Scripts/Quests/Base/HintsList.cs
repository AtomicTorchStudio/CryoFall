namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class HintsList
    {
        private static readonly HintsList TempList = new HintsList();

        private readonly List<string> list = new List<string>();

        public static HintsList GetTempList()
        {
            TempList.Clear();
            return TempList;
        }

        public HintsList Add(string hint)
        {
            this.list.Add(hint);
            return this;
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public override string ToString()
        {
            if (this.list.Count == 0)
            {
                return null;
            }

            var result = this.list
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