namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ConsoleControlSuggestionsProvider
    {
        private readonly TextBox inputTextBox;

        private readonly TextBlock textBlockSuggestionGhost;

        private readonly ViewModelConsoleControl viewModelConsoleControl;

        private ConsoleCommandVariant currentCommandVariant;

        private IReadOnlyList<string> currentSuggestions;

        private byte lastSuggestionRequestId;

        private string lastText;

        private int suggestionCurrentIndex;

        public ConsoleControlSuggestionsProvider(
            TextBox inputTextBox,
            TextBlock textBlockSuggestionGhost,
            ViewModelConsoleControl viewModelConsoleControl)
        {
            this.inputTextBox = inputTextBox;
            this.textBlockSuggestionGhost = textBlockSuggestionGhost;
            this.viewModelConsoleControl = viewModelConsoleControl;
            ConsoleCommandsSystem.ClientSuggestionsCallback = this.SuggestionsProvidedHandler;
        }

        public bool IsSuggestionMode { get; private set; }

        public void ExitSuggestionMode(bool resetSuggestions)
        {
            this.IsSuggestionMode = false;

            if (resetSuggestions)
            {
                this.currentSuggestions = null;
                this.viewModelConsoleControl.SetSuggestions(null);
            }
        }

        public void OnTab()
        {
            if (this.IsSuggestionMode)
            {
                this.SelectNextOrPreviousSuggestion();
                return;
            }

            // request suggestion
            this.IsSuggestionMode = true;
            //this.lastText = null;
            this.RequestSuggestions();
        }

        public void OnTextChanged()
        {
            this.RequestSuggestions();
        }

        public bool SelectSuggestion(bool isPreviousSuggestion, int itemsDistance = 1)
        {
            if (this.currentSuggestions is null
                || this.currentSuggestions.Count <= 0)
            {
                return false;
            }

            if (isPreviousSuggestion)
            {
                // switch back
                this.suggestionCurrentIndex -= itemsDistance;
                if (this.suggestionCurrentIndex < 0)
                {
                    if (itemsDistance == 1)
                    {
                        // cycle from end
                        this.suggestionCurrentIndex = this.currentSuggestions.Count - 1;
                    }
                    else
                    {
                        // stuck in end
                        this.suggestionCurrentIndex = 0;
                    }
                }
            }
            else
            {
                // switch forward
                this.suggestionCurrentIndex += itemsDistance;
                if (this.suggestionCurrentIndex > this.currentSuggestions.Count - 1)
                {
                    if (itemsDistance == 1)
                    {
                        // cycle from beginning
                        this.suggestionCurrentIndex = 0;
                    }
                    else
                    {
                        // stuck in beginning
                        this.suggestionCurrentIndex = this.currentSuggestions.Count - 1;
                    }
                }
            }

            this.viewModelConsoleControl.SuggestionsListSelectedItemIndex = this.suggestionCurrentIndex;

            var suggestedText = this.currentSuggestions.Count > 0
                                    ? this.currentSuggestions[this.suggestionCurrentIndex]
                                    : string.Empty;

            this.SetSuggestedText(suggestedText);
            return true;
        }

        public void UpdateSuggestionGhostOnly()
        {
            var text = this.inputTextBox.Text;
            if (text.Length == 0)
            {
                this.textBlockSuggestionGhost.Text = string.Empty;
                return;
            }

            var stringBuilder = new StringBuilder(capacity: 100);
            string commandName;
            if (this.currentCommandVariant is null)
            {
                // get suggested command name
                if (text[0] == ConsoleCommandsSystem.ServerConsoleCommandPrefixOnClient)
                {
                    // cannot suggest server commands on client
                }
                else
                {
                    commandName = ConsoleCommandsSystem.SharedGetCommandNamesSuggestions(text)
                                                       .FirstOrDefault()?.GetNameOrAlias(text);
                    stringBuilder.Append(commandName);
                }

                this.textBlockSuggestionGhost.Text = stringBuilder.ToString();
                return;
            }

            var consoleCommand = this.currentCommandVariant.ConsoleCommand;
            {
                var name = text.TrimStart(ConsoleCommandsSystem.ServerConsoleCommandPrefixOnClient);

                var indexOfSpace = name.IndexOf(' ');
                if (indexOfSpace > 0)
                {
                    name = name.Substring(0, indexOfSpace);
                }

                if (!consoleCommand.Name.StartsWith(name)
                    && (consoleCommand.Alias is null
                        || !consoleCommand.Alias.StartsWith(name)))
                {
                    // current command variant is different from what is displayed now
                    // remove ghost
                    this.textBlockSuggestionGhost.Text = string.Empty;
                    return;
                }
            }

            ConsoleCommandParser.ParseCommandNameAndArguments(
                text,
                textPosition: (ushort)this.inputTextBox.CaretIndex,
                commandName: out commandName,
                arguments: out var arguments,
                argumentIndexForSuggestion: out var argumentIndexForSuggestion);

            if (text[0] == ConsoleCommandsSystem.ServerConsoleCommandPrefixOnClient)
            {
                // append server command prefix
                stringBuilder.Append(ConsoleCommandsSystem.ServerConsoleCommandPrefixOnClient);
                text = text.Substring(1);
            }

            var isCommandNameParsedSuccessfully = this.currentCommandVariant is not null
                                                  && (commandName.Equals(consoleCommand.Name,
                                                                         StringComparison.OrdinalIgnoreCase)
                                                      || commandName.Equals(
                                                          consoleCommand.Alias,
                                                          StringComparison.OrdinalIgnoreCase));
            if (!isCommandNameParsedSuccessfully)
            {
                // need to print ghost for the command name
                commandName = consoleCommand.GetNameOrAlias(commandName);
                stringBuilder.Append(commandName)
                             .Append(' ');
            }
            else
            {
                // no need to print any ghost before for the current auto-complete argument
                // add padding before this argument
                var paddingCount = text.TrimEnd(' ').Length + 1;
                stringBuilder.Append(' ', repeatCount: paddingCount);
            }

            // skip all parameters before latest entered and then append the "ghost text" for all the remaining parameters
            var parametersToSkipCount = Math.Max(arguments.Count(a => !string.IsNullOrEmpty(a)),
                                                 argumentIndexForSuggestion);
            var parameters = this.currentCommandVariant.Parameters;

            {
                var isFirst = true;
                foreach (var commandParameter in parameters.Skip(parametersToSkipCount))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        stringBuilder.Append(' ');
                    }

                    stringBuilder.Append(commandParameter.Name);
                }
            }

            this.textBlockSuggestionGhost.Text = stringBuilder.ToString();
        }

        /// <summary>
        /// Request suggestions - it will return matched command variant and current parameter suggestions.
        /// </summary>
        private void RequestSuggestions()
        {
            var text = this.inputTextBox.Text;
            if (this.lastText == text)
            {
                // already requested the same suggestion
                if (this.currentCommandVariant is not null)
                {
                    this.SuggestionsProvidedHandler(this.currentCommandVariant,
                                                    this.currentSuggestions,
                                                    this.lastSuggestionRequestId);
                }

                return;
            }

            //this.currentSuggestions = null;
            //this.viewModelConsoleControl.SetSuggestions(null);

            this.lastText = text;
            var textPosition = (ushort)this.inputTextBox.CaretIndex;
            var requestId = ++this.lastSuggestionRequestId;
            ConsoleCommandsSystem.ClientSuggestAutocomplete(text, textPosition, requestId);
        }

        private void SelectNextOrPreviousSuggestion()
        {
            // switch between suggestions
            var isPreviousSuggestion = Api.Client.Input.IsKeyHeld(InputKey.Shift, evenIfHandled: true);
            this.SelectSuggestion(isPreviousSuggestion);
        }

        /// <summary>
        /// Swaps commandname or argument text with the suggested text.
        /// </summary>
        /// <param name="suggestedText"></param>
        private void SetSuggestedText(string suggestedText)
        {
            if (suggestedText.Length == 0)
            {
                this.UpdateSuggestionGhostOnly();
                return;
            }

            var isNeedServerCommandPrefix = false;
            var text = new StringBuilder(this.inputTextBox.Text);

            if (text.Length > 0
                && text[0] == ConsoleCommandsSystem.ServerConsoleCommandPrefixOnClient)
            {
                isNeedServerCommandPrefix = true;
            }

            ConsoleCommandParser.ParseCommandNameAndArguments(
                text.ToString(),
                (ushort)this.inputTextBox.CaretIndex,
                out var parsedCommandName,
                out var parsedArguments,
                out var parsedArgumentIndexForSuggestion);

            text.Clear();

            // reconstruct command
            if (isNeedServerCommandPrefix)
            {
                text.Append(ConsoleCommandsSystem.ServerConsoleCommandPrefixOnClient);
            }

            int? caretIndex = null;
            if (parsedArgumentIndexForSuggestion == -1
                || this.currentCommandVariant is null
                || (!string.Equals(parsedCommandName,
                                   this.currentCommandVariant.ConsoleCommand.Name,
                                   StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(parsedCommandName,
                                      this.currentCommandVariant.ConsoleCommand.Alias,
                                      StringComparison.OrdinalIgnoreCase)))
            {
                // command name is suggested
                parsedCommandName = suggestedText;
                parsedArguments = Array.Empty<string>();
            }

            text.Append(parsedCommandName);

            for (var index = 0; index < parsedArguments.Length; index++)
            {
                text.Append(' ');

                // get argument text
                var argument = index == parsedArgumentIndexForSuggestion
                                   ? suggestedText // argument is suggested
                                   : parsedArguments[index];

                var isQuotedArgument = argument.IndexOf(' ') > -1;
                if (isQuotedArgument)
                {
                    text.Append('\"');
                }

                text.Append(argument);
                if (isQuotedArgument)
                {
                    text.Append('\"');
                }

                if (index == parsedArgumentIndexForSuggestion)
                {
                    caretIndex = text.Length;
                }
            }

            this.inputTextBox.Text = text.ToString();
            this.inputTextBox.CaretIndex = caretIndex ?? text.Length;

            this.UpdateSuggestionsMenuAndGhost();
        }

        private void SuggestionsProvidedHandler(
            ConsoleCommandVariant commandVariant,
            IReadOnlyList<string> suggestions,
            byte requestId)
        {
            if (this.lastSuggestionRequestId != requestId)
            {
                // not the callback on the latest request - ignore it
                return;
            }

            this.currentCommandVariant = commandVariant;

            if (suggestions is not null)
            {
                var sortedSuggestions = new List<string>(suggestions);
                sortedSuggestions.Sort(StringComparer.OrdinalIgnoreCase);
                this.currentSuggestions = sortedSuggestions;
            }
            else
            {
                this.currentSuggestions = null;
            }

            this.suggestionCurrentIndex = 0;
            this.viewModelConsoleControl.SetSuggestions(this.currentSuggestions);

            if (!this.IsSuggestionMode)
            {
                this.UpdateSuggestionsMenuAndGhost();
                return;
            }

            // select suggested text
            var suggestion = this.currentSuggestions?.Count > 0
                                 ? this.currentSuggestions[this.suggestionCurrentIndex]
                                 : string.Empty;

            this.SetSuggestedText(suggestion);

            if (this.currentSuggestions is not null
                && this.currentSuggestions.Count == 1)
            {
                // reset suggestions as there are no other suggestions except the one which was set right now
                this.ExitSuggestionMode(resetSuggestions: true);
            }
        }

        private void UpdateSuggestionsMenuAndGhost()
        {
            var isNeedServerCommandPrefix = false;
            var text = new StringBuilder(this.inputTextBox.Text);

            if (text.Length > 0
                && text[0] == ConsoleCommandsSystem.ServerConsoleCommandPrefixOnClient)
            {
                isNeedServerCommandPrefix = true;
            }

            ConsoleCommandParser.ParseCommandNameAndArguments(
                text.ToString(),
                (ushort)this.inputTextBox.CaretIndex,
                out var commandName,
                out var arguments,
                out var argumentIndexForSuggestion);

            text.Clear();

            // reconstruct command
            if (isNeedServerCommandPrefix)
            {
                text.Append(ConsoleCommandsSystem.ServerConsoleCommandPrefixOnClient);
            }

            var suggestionsListControlCharsOffset = 0;
            text.Append(commandName);

            for (var index = 0; index < arguments.Length; index++)
            {
                // get argument text
                var argument = index == argumentIndexForSuggestion
                                   ? commandName // argument is suggested
                                   : arguments[index];

                text.Append(' ');

                var isQuotedArgument = argument.IndexOf(' ') > -1;
                if (isQuotedArgument)
                {
                    text.Append('\"');
                }

                text.Append(argument);
                if (isQuotedArgument)
                {
                    text.Append('\"');
                }

                if (index == argumentIndexForSuggestion)
                {
                    suggestionsListControlCharsOffset = text.Length - argument.Length - (isQuotedArgument ? 2 : 0);
                }
            }

            if (isNeedServerCommandPrefix && argumentIndexForSuggestion == -1)
            {
                // add extra padding (server command name)
                suggestionsListControlCharsOffset++;
            }

            this.viewModelConsoleControl.SetSuggestionsControlOffset(suggestionsListControlCharsOffset);
            this.UpdateSuggestionGhostOnly();
        }
    }
}