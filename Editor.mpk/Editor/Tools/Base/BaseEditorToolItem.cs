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
                var indexOfParenthesis = name.IndexOf('(');
                if (indexOfParenthesis > 0)
                {
                    var result = name.Substring(0, 5);
                    result += "\n";
                    result += name.Substring(indexOfParenthesis + 1,
                                             length: Math.Min(5, name.Length - (indexOfParenthesis + 1)))
                                  .TrimEnd(')');
                    return result;
                }

                return name.Length > 5
                           ? name.Substring(0, 5)
                           : name;
            }
        }
    }
}