namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Collections.Generic;
    using System.Text;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Extensions;

    public class CommandParameter
    {
        private readonly CustomSuggestionsAttribute customSuggestionsAttribute;

        private readonly bool hasCurrentCharacterAttribute;

        private readonly ScriptingParameterInfo parameterInfo;

        private string name;

        public CommandParameter(ScriptingParameterInfo parameterInfo)
        {
            this.parameterInfo = parameterInfo;

            this.hasCurrentCharacterAttribute =
                this.parameterInfo.GetCustomAttribute(typeof(CurrentCharacterIfNullAttribute))
                != null;

            this.customSuggestionsAttribute = (CustomSuggestionsAttribute)
                this.parameterInfo.GetCustomAttribute(typeof(CustomSuggestionsAttribute));
        }

        public bool IsOptional => this.parameterInfo.ParameterInfo.IsOptional
                                  || this.hasCurrentCharacterAttribute;

        public string Name => this.name ?? (this.name = this.CreateName());

        public bool GetDefaultValue(ICharacter byCharacter, out object value)
        {
            if (this.hasCurrentCharacterAttribute)
            {
                if (byCharacter == null)
                {
                    // current character is not provided (system console on server)
                    value = null;
                    return false;
                }

                value = byCharacter;
                return true;
            }

            if (this.parameterInfo.ParameterInfo.HasDefaultValue)
            {
                value = this.parameterInfo.ParameterInfo.DefaultValue;
                return true;
            }

            value = null;
            return false;
        }

        public IEnumerable<string> GetSuggestions(string startsWith)
        {
            if (this.customSuggestionsAttribute != null)
            {
                return this.customSuggestionsAttribute
                           .GetSuggestions(this.parameterInfo.ParameterInfo.Member.DeclaringType,
                                           startsWith);
            }

            return ConsoleCommandsParametersHelper.GetSuggestions(
                startsWith,
                this.parameterInfo.ParameterInfo.ParameterType);
        }

        public bool ParseArgument(string value, out object result)
        {
            return ConsoleCommandsParametersHelper.ParseArgument(
                value,
                this.parameterInfo.ParameterInfo.ParameterType,
                out result);
        }

        public override string ToString()
        {
            return this.Name;
        }

        private string CreateName()
        {
            var info = this.parameterInfo.ParameterInfo;
            var isOptional = this.hasCurrentCharacterAttribute || info.HasDefaultValue;
            var stringBuilder = new StringBuilder(255);
            stringBuilder.Append(isOptional ? '[' : '<');
            stringBuilder.Append(
                ConsoleCommandsParametersHelper.GetParameterDescription(
                    info.ParameterType,
                    info.Name));
            stringBuilder.Append(isOptional ? ']' : '>');
            return stringBuilder.ToString();
        }
    }
}