namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Base
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class BaseEditorToolItem
    {
        private readonly bool displayShortName;

        protected BaseEditorToolItem(string name, string id, bool displayShortName)
        {
            this.displayShortName = displayShortName;
            this.Name = name;
            this.Id = id;
        }

        public abstract ITextureResource Icon { get; }

        public string Id { get; }

        public string Name { get; }

        public string ShortName
        {
            get
            {
                if (!this.displayShortName)
                {
                    return null;
                }

                var name = this.Name;
                var indexOfSplit = name.IndexOf('(');
                var maxLength = 8;

                if (indexOfSplit > 0)
                {
                    var result = name.Substring(0, Math.Min(maxLength, indexOfSplit - 1));
                    result += "\n";
                    result += name.Substring(indexOfSplit + 1,
                                             length: Math.Min(maxLength, name.Length - (indexOfSplit + 1)))
                                  .TrimEnd(')');
                    return result;
                }

                return name.Length > maxLength
                           ? name.Substring(0, maxLength)
                           : name;
            }
        }
    }
}