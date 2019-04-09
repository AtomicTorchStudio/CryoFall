namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Extensions;
    using JetBrains.Annotations;

    public class CustomSuggestionsAttribute : Attribute
    {
        private readonly string methodName;

        private CustomSuggestionsDelegate cachedFuncGetSuggestions;

        private Type cachedType;

        public CustomSuggestionsAttribute([NotNull] string methodName)
        {
            this.methodName = methodName;
        }

        public delegate IEnumerable<string> CustomSuggestionsDelegate(string startsWith);

        public IEnumerable<string> GetSuggestions(Type type, string startsWith)
        {
            if (this.cachedFuncGetSuggestions == null)
            {
                // cache function delegate
                this.cachedType = type;
                this.cachedFuncGetSuggestions = (CustomSuggestionsDelegate)
                    type.ScriptingGetMethod(this.methodName)
                        .CreateDelegate(typeof(CustomSuggestionsDelegate));
            }
            else if (this.cachedType != type)
            {
                throw new Exception("Cached method is for the another type");
            }

            return this.cachedFuncGetSuggestions(startsWith);
        }
    }
}