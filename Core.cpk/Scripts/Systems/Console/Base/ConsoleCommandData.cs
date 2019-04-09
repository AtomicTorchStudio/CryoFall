namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleCommandData
    {
        public readonly BaseConsoleCommand ConsoleCommand;

        private readonly byte argumentIndexForSuggestion;

        private readonly string[] arguments;

        public ConsoleCommandData(
            BaseConsoleCommand consoleCommand,
            string[] arguments,
            byte argumentIndexForSuggestion)
        {
            this.ConsoleCommand = consoleCommand;
            this.arguments = arguments;
            this.argumentIndexForSuggestion = argumentIndexForSuggestion;
        }

        public void Execute(ICharacter byCharacter)
        {
            this.ConsoleCommand.ExecuteCommand(byCharacter, this.arguments);
        }

        public string[] GetParameterSuggestions(ICharacter byCharacter, out ConsoleCommandVariant consoleCommandVariant)
        {
            return this.ConsoleCommand.GetParameterSuggestions(
                byCharacter,
                this.arguments,
                this.argumentIndexForSuggestion,
                out consoleCommandVariant);
        }
    }
}