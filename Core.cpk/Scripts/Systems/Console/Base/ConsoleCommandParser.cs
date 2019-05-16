namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Collections.Generic;
    using System.Text;

    public static class ConsoleCommandParser
    {
        private static readonly string[] EmptyArguments = new string[0];

        public static void ParseCommandNameAndArguments(
            string text,
            ushort textPosition,
            out string commandName,
            out string[] arguments,
            out short argumentIndexForSuggestion)
        {
            commandName = string.Empty;
            arguments = EmptyArguments;
            short currentArgumentIndex = -1;
            argumentIndexForSuggestion = -1;

            var argumentsList = new List<string>();
            var buffer = new StringBuilder(capacity: text.Length / 2);

            // determines if currently parsing the argument within "quotes possibly with spaces".
            var isParsingArgumentWithinQuotes = false;

            // init with true to trim extra spaces/padding in begin of the text
            var isPreviousCharWasPadding = true;

            for (var index = 0; index < text.Length; index++)
            {
                var c = text[index];

                if (textPosition == index)
                {
                    argumentIndexForSuggestion = currentArgumentIndex;
                }

                switch (c)
                {
                    case ConsoleCommandsSystem.ServerConsoleCommandPrefixOnClient:
                    {
                        // do not append this char
                        continue;
                    }

                    case ' ':
                        if (isParsingArgumentWithinQuotes)
                        {
                            // append space char as is
                            buffer.Append(c);
                            isPreviousCharWasPadding = false;
                            continue;
                        }

                        if (isPreviousCharWasPadding)
                        {
                            // skip this character
                            continue;
                        }

                        isPreviousCharWasPadding = true;
                        if (currentArgumentIndex == -1)
                        {
                            commandName = buffer.ToString();
                        }
                        else
                        {
                            argumentsList.Add(buffer.ToString());
                        }

                        buffer.Clear();
                        currentArgumentIndex++;
                        continue;

                    case '\"':
                        // parse quoted argument
                        if (currentArgumentIndex == -1)
                        {
                            // quotes starting before first argument!
                            return;
                        }

                        isPreviousCharWasPadding = true;
                        if (!isParsingArgumentWithinQuotes)
                        {
                            isParsingArgumentWithinQuotes = true;
                            continue;
                        }

                        // capture value as argument
                        isParsingArgumentWithinQuotes = false;
                        argumentsList.Add(buffer.ToString());
                        buffer.Clear();
                        currentArgumentIndex++;
                        continue;

                    default:
                        buffer.Append(c);
                        isPreviousCharWasPadding = false;
                        continue;
                }
            }

            if (commandName.Length == 0)
            {
                // command name was not parsed - it means all the text in buffer is command name
                commandName = buffer.ToString();
            }
            else
            {
                // add buffer content as the last argument
                argumentsList.Add(buffer.ToString());
            }

            if (text.Length > 0
                && textPosition > text.Length - 1)
            {
                // suggest for not added argument
                argumentIndexForSuggestion = currentArgumentIndex;
            }

            arguments = argumentsList.Count > 0 ? argumentsList.ToArray() : EmptyArguments;
        }
    }
}