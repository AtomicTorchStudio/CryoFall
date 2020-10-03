// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using JetBrains.Annotations;

    // we mark it as used implicitly with members to avoid ReSharper marking "Execute*" methods as not used
    [UsedImplicitly(targetFlags: ImplicitUseTargetFlags.WithMembers)]
    public abstract class BaseConsoleCommand
    {
        protected BaseConsoleCommand()
        {
            var type = this.GetType();
            this.Id = type.FullName;
            this.ShortId = type.Name;

            // ReSharper disable once VirtualMemberCallInConstructor
            if (this.Name.Contains(' '))
            {
                throw new Exception(
                    "Command Name property should not contain spaces - " + this.ShortId + ": Name=>" + this.Name);
            }
        }

        public virtual string Alias { get; }

        public abstract string Description { get; }

        public string Id { get; }

        public abstract ConsoleCommandKinds Kind { get; }

        public abstract string Name { get; }

        public string ShortId { get; }

        public IReadOnlyList<ConsoleCommandVariant> Variants { get; private set; }

        /// <summary>
        /// Gets the ClientScriptingApi - use only on Client-side.
        /// </summary>
        protected static IClientApi Client => Api.Client;

        /// <summary>
        /// Returns true if current code is executing on Client-side.
        /// </summary>
        protected static bool IsClient => Api.IsClient;

        /// <summary>
        /// Returns true if current code is executing on Server-side.
        /// </summary>
        protected static bool IsServer => Api.IsServer;

        /// <summary>
        /// Gets the logger instance.
        /// </summary>
        protected static ILogger Logger => Api.Logger;

        /// <summary>
        /// Returns ServerApi - use only on Server-side.
        /// </summary>
        protected static IServerApi Server => Api.Server;

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
            if (method is null)
            {
                Logger.Warning("Incorrect command syntax.");
                return;
            }

            try
            {
                this.ExecutionContextCurrentCharacter = byCharacter;
                this.ValidateAndFixArguments(ref arguments, method.Parameters);

                method.Execute(byCharacter, arguments);
            }
            catch (ConsoleCommandParsingException exception)
            {
                ConsoleCommandsSystem.Instance.ServerOnConsoleCommandError(byCharacter, exception.Message);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Warning(
                    $"Error during processing the console command {this.Name} {arguments.GetJoinedString(" ")}: "
                    + ex.Message,
                    characterRelated: byCharacter);
            }
            finally
            {
                this.ExecutionContextCurrentCharacter = null;
            }
        }

        public string GetNameOrAlias(string startsWith)
        {
            if (this.Alias is not null
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
            if (bestMatchedVariant is null)
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
            if (this.Variants is not null)
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
            var isServerCommandRequiresModerator = (this.Kind & ConsoleCommandKinds.ServerModerator) != 0;

            if (!isServerCommandForEveryone
                && !isServerCommandRequiresOperator
                && !isServerCommandRequiresModerator)
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

            if (isServerCommandRequiresModerator)
            {
                var isModerator = ConsoleCommandsSystem.ServerIsModeratorOrSystemConsole(byCharacter);
                if (!isModerator)
                {
                    throw new Exception("You need a moderator operator access to execute this command: " + this.Name);
                }
            }
        }

        /// <summary>
        /// Gets the instances of proto-classes of the specified type. For example, use IItemType as type parameter to get all
        /// proto-classes of IItemType.
        /// </summary>
        /// <typeparam name="TProtoEntity">Type of proto entity.</typeparam>
        /// <returns>Collection of instances which implements specified prototype.</returns>
        protected static List<TProtoEntity> FindProtoEntities<TProtoEntity>()
            where TProtoEntity : class, IProtoEntity
        {
            return Api.FindProtoEntities<TProtoEntity>();
        }

        /// <summary>
        /// Gets the instance of proto-class by its type.
        /// </summary>
        /// <typeparam name="TProtoEntity">Type of proto entity.</typeparam>
        /// <returns>Instance of proto-class.</returns>
        protected static TProtoEntity GetProtoEntity<TProtoEntity>()
            where TProtoEntity : class, IProtoEntity, new()
        {
            return Api.GetProtoEntity<TProtoEntity>();
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
                else if (bestMatchMethod is null)
                    //&& method.RequiredParametersCount == 0)
                {
                    // method without parameters
                    bestMatchMethod = method;
                }
            }

            return bestMatchMethod;
        }

        private void ValidateAndFixArguments(ref string[] arguments, IReadOnlyList<CommandParameter> methodParameters)
        {
            if (arguments.Length <= methodParameters.Count)
            {
                return;
            }

            // Hack to prevent case when a command like this
            // will not work due to an empty last argument:
            // /admin.notifyAll "Test" 
            for (var index = methodParameters.Count; index < arguments.Length; index++)
            {
                var argument = arguments[index];
                if (!string.IsNullOrEmpty(argument))
                {
                    throw new ConsoleCommandParsingException(
                        "There is an extra argument which is not required for the console command. Please ensure that you're entering the arguments correctly!");
                }
            }

            arguments = arguments.Take(methodParameters.Count)
                                 .ToArray();
        }
    }
}