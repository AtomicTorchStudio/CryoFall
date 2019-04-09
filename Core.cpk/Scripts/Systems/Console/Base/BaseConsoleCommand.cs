// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Extensions;
    using JetBrains.Annotations;

    // we mark it as used implicitly with members to avoid ReSharper marking "Execute*" methods as not used
    [UsedImplicitly(targetFlags: ImplicitUseTargetFlags.WithMembers)]
    public abstract class BaseConsoleCommand : ProtoEntity
    {
        public virtual string Alias { get; }

        public abstract string Description { get; }

        public abstract ConsoleCommandKinds Kind { get; }

        public IReadOnlyList<ConsoleCommandVariant> Variants { get; private set; }

        /// <summary>
        /// Gets the character executing this command
        /// (can be null if the command is executed directly from the server console).
        /// </summary>
        [CanBeNull]
        protected ICharacter ExecutionContextCurrentCharacter { get; private set; }

        public void ExecuteCommand([NotNull] ICharacter byCharacter, string[] arguments)
        {
            this.Validate(byCharacter);
            var method = this.MatchVariant(byCharacter, arguments);
            if (method == null)
            {
                Logger.Warning("Incorrect command syntax.");
                return;
            }

            try
            {
                this.ExecutionContextCurrentCharacter = byCharacter;
                method.Execute(byCharacter, arguments);
            }
            finally
            {
                this.ExecutionContextCurrentCharacter = null;
            }
        }

        public string GetNameOrAlias(string startsWith)
        {
            if (this.Alias != null
                && this.Alias.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
            {
                return this.Alias;
            }

            return this.Name;
        }

        public string[] GetParameterSuggestions(
            [NotNull] ICharacter byCharacter,
            string[] arguments,
            byte argumentIndex,
            out ConsoleCommandVariant consoleCommandVariant)
        {
            this.Validate(byCharacter);

            var bestMatchedVariant = this.MatchVariant(byCharacter, arguments);
            if (bestMatchedVariant == null)
            {
                consoleCommandVariant = null;
                return null;
            }

            var startsWith = argumentIndex < arguments.Length
                                 ? arguments[argumentIndex]
                                 : string.Empty;

            consoleCommandVariant = bestMatchedVariant;
            return bestMatchedVariant.GetParameterSuggestions(startsWith, argumentIndex);
        }

        public void InitializeIfRequired()
        {
            if (this.Variants != null)
            {
                return;
            }

            var variants = this.GetType()
                               .ScriptingGetMethods(includePrivate: false)
                               .Where(IsExecuteMethod)
                               .Select(m => new ConsoleCommandVariant(this, m))
                               .OrderBy(m => m.Parameters.Count)
                               .ToList();
            this.Variants = variants;

            if (this.Variants.Count == 0)
            {
                Logger.Error("No public Execute() method found in the command type " + this.ShortId);
            }

            // helper local function for checking the execute methods
            bool IsExecuteMethod(ScriptingMethodInfo m)
            {
                var methodName = m.MethodInfo.Name;
                return methodName.StartsWith("Execute", StringComparison.Ordinal)
                       && !methodName.Equals("ExecuteCommand", StringComparison.Ordinal);
            }
        }

        public void Validate(ICharacter byCharacter)
        {
            if (IsClient)
            {
                if ((this.Kind & ConsoleCommandKinds.Client) == 0)
                {
                    throw new Exception("Cannot execute server command on Client-side: " + this.Name);
                }

                // can execute
                return;
            }

            // if server
            var isServerCommandForEveryone = (this.Kind & ConsoleCommandKinds.ServerEveryone) != 0;
            var isServerCommandRequiresOperator = (this.Kind & ConsoleCommandKinds.ServerOperator) != 0;

            if (!isServerCommandForEveryone
                && !isServerCommandRequiresOperator)
            {
                throw new Exception("Cannot execute client command on Server-side: " + this.Name);
            }

            if (isServerCommandRequiresOperator)
            {
                var isOperator = ConsoleCommandsSystem.ServerIsOperatorOrSystemConsole(byCharacter);
                if (!isOperator)
                {
                    throw new Exception("You need a server operator access to execute this command: " + this.Name);
                }
            }
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();

            if (this.Name.Contains(' '))
            {
                throw new Exception(
                    "Command Name property should not contain spaces - " + this.ShortId + ": Name=>" + this.Name);
            }
        }

        private ConsoleCommandVariant MatchVariant(ICharacter byCharacter, string[] arguments)
        {
            this.InitializeIfRequired();

            var bestMatchRating = 0;
            ConsoleCommandVariant bestMatchMethod = null;
            foreach (var method in this.Variants)
            {
                var rating = method.GetMatchRating(byCharacter, arguments);
                if (bestMatchRating < rating)
                {
                    bestMatchRating = rating;
                    bestMatchMethod = method;
                }
                else if (bestMatchMethod == null)
                    //&& method.RequiredParametersCount == 0)
                {
                    // method without parameters
                    bestMatchMethod = method;
                }
            }

            return bestMatchMethod;
        }
    }
}