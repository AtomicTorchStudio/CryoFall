namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Documents;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    internal static class ClientTextTagFormatter
    {
        private static readonly char[] EndUrlSeparators = { ' ', '\r', '\n' };

        public static FormattedTextBlock NewFormattedTextBlock(string text)
        {
            return new FormattedTextBlock() { Content = text };
        }

        public static List<Inline> ParseInlines(string text)
        {
            try
            {
                var result = new List<Inline>();
                var rawText = ReflowText(text);

                foreach (var inline in BuildInlines(rawText))
                {
                    if (inline != null)
                    {
                        result.Add(inline);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Api.Logger.Exception(ex);
                return new List<Inline>()
                {
                    new Run(
                        "CANNOT PARSE the text tags. The exception is logged. Below is the raw text:"
                        + Environment.NewLine
                        + text)
                };
            }
        }

        private static IEnumerable<Inline> BuildInlines(string text)
        {
            var startPosition = 0;
            int position;

            var isTagParsingMode = false;

            var modeIsBold = false;
            var modeIsItalic = false;
            var modeIsUnderline = false;
            string modeUrlCurrentUrl = null;

            for (position = 0; position < text.Length; position++)
            {
                var ch = text[position];
                switch (ch)
                {
                    // tag opening bracket
                    case '[':
                        if (isTagParsingMode)
                        {
                            throw new Exception(
                                $"Incorrect text - found new tag opening bracket ([) when expected closed bracket (])Text: {text}");
                        }

                        yield return TryBuildRun();

                        isTagParsingMode = true;
                        startPosition = position + 1;
                        break;

                    // tag closing bracket
                    case ']':
                        if (!isTagParsingMode)
                        {
                            throw new Exception(
                                $"Incorrect text - found tag closing bracket ([) when there is no opening bracket ([){Environment.NewLine}Text: {text}");
                        }

                        // ended parsing the tag
                        isTagParsingMode = false;
                        var tag = TakeAccumulatedString();

                        if (tag.StartsWith("url", StringComparison.Ordinal))
                        {
                            if (modeUrlCurrentUrl != null)
                            {
                                throw new Exception(
                                    "Nested [url] elements are not supported. Issue with formatted string:"
                                    + Environment.NewLine
                                    + text);
                            }

                            yield return TryBuildRun();

                            // extract url
                            var url = tag.Substring(tag.IndexOf('=') + 1);
                            modeUrlCurrentUrl = url;
                        }
                        else
                        {
                            switch (tag)
                            {
                                case "b":
                                    yield return TryBuildRun();

                                    modeIsBold = true;
                                    break;

                                case "/b":
                                    yield return TryBuildRun();

                                    modeIsBold = false;
                                    break;

                                case "i":
                                    yield return TryBuildRun();

                                    modeIsItalic = true;
                                    break;

                                case "/i":
                                    yield return TryBuildRun();

                                    modeIsItalic = false;
                                    break;

                                case "u":
                                    yield return TryBuildRun();

                                    modeIsUnderline = true;
                                    break;

                                case "/u":
                                    yield return TryBuildRun();

                                    modeIsUnderline = false;
                                    break;

                                case "/url":
                                    yield return TryBuildRun();

                                    modeUrlCurrentUrl = null;
                                    break;

                                // add bullet point
                                case "*":
                                    yield return TryBuildRun();

                                    if (position > "[*]".Length)
                                    {
                                        // auto-insert empty line
                                        yield return new LineBreak();
                                    }

                                    yield return new Run("\u2022") { FontWeight = FontWeights.Bold };

                                    // check next char
                                    if (position + 1 < text.Length
                                        && text[position + 1] != ' ')
                                    {
                                        // next char is not an empty space - add space char
                                        yield return new Run(" ");
                                    }

                                    break;

                                // add line break
                                case "br":
                                    yield return TryBuildRun();
                                    yield return new LineBreak();

                                    if (position + 1 < text.Length
                                        && text[position + 1] == ' ')
                                    {
                                        // skip one space after [br] tag
                                        position++;
                                    }

                                    break;

                                default:
                                    Api.Logger.Error(
                                        $"Unknown tag in text: tag - {tag}"
                                        + $"{Environment.NewLine}Text: {text}");
                                    break;
                            }
                        }

                        // set next position to be the start position
                        startPosition = position + 1;
                        break;

                    // any other char
                    default:
                        // don't do anything
                        break;
                }
            }

            yield return TryBuildRun();

            string TakeAccumulatedString()
            {
                if (startPosition == position)
                {
                    return string.Empty;
                }

                var result = text.Substring(startPosition, position - startPosition);
                startPosition = position;
                return result;
            }

            // helper local function for building text run
            Inline TryBuildRun()
            {
                if (isTagParsingMode)
                {
                    throw new InvalidOperationException();
                }

                var lastStartPosition = startPosition;
                var lastEndPosition = position;

                var strPart = TakeAccumulatedString();
                if (strPart.Length == 0)
                {
                    return null;
                }

                var span = new Span();
                if (modeIsBold)
                {
                    span.FontWeight = FontWeights.Bold;
                }

                if (modeIsItalic)
                {
                    // actually our fonts rarely has italic variant
                    span.FontStyle = FontStyles.Italic;
                }

                if (modeIsUnderline)
                {
                    // TODO: due to the scan-lines effect in the DataLog window the underline might be invisible
                    // currently there is no way of regulating the underline thickness
                    // it's also impossible to create custom text decorations as TextDecorations in Noesis is an enum.
                    span.TextDecorations = TextDecorations.Underline;
                }

                var spanInlines = span.Inlines;

                if (modeUrlCurrentUrl != null)
                {
                    spanInlines.Add(new Run(strPart));
                    // currently parsing an [url=...] tag content - wrap span into a hyperlink
                    span = CreateHyperlinkForUrl(span, modeUrlCurrentUrl);
                }
                else
                {
                    // try extract hyperlink
                    var indexOfHttp = strPart.IndexOf("http://",   StringComparison.Ordinal);
                    var indexOfHttps = strPart.IndexOf("https://", StringComparison.Ordinal);
                    if (indexOfHttp >= 0
                        || indexOfHttps >= 0)
                    {
                        // found hyperlink content
                        if (indexOfHttp < 0)
                        {
                            indexOfHttp = int.MaxValue;
                        }

                        if (indexOfHttps < 0)
                        {
                            indexOfHttps = int.MaxValue;
                        }

                        var urlStartIndex = Math.Min(indexOfHttp, indexOfHttps);
                        BuildHyperlinkForUrl(spanInlines,
                                             lastStartPosition,
                                             lastEndPosition,
                                             strPart,
                                             urlStartIndex);
                    }
                    else
                    {
                        // no hyperlink content
                        spanInlines.Add(new Run(strPart));
                    }
                }

                return span;
            }

            void BuildHyperlinkForUrl(
                InlineCollection spanInlines,
                int lastStartPosition,
                int lastEndPosition,
                string strPart,
                int indexOfHttp)
            {
                // parse and add prefix part (if any)
                startPosition = lastStartPosition;
                position = lastStartPosition + indexOfHttp;
                var prefix = TryBuildRun();
                if (prefix != null)
                {
                    spanInlines.Add(prefix);
                }

                // parse and add hyperlink
                spanInlines.Add(
                    ExtractHyperlink(strPart, indexOfHttp, out var urlLength));

                // parse and add suffix part (if any)
                startPosition = lastStartPosition + indexOfHttp + urlLength;
                position = lastEndPosition;
                var suffix = TryBuildRun();
                if (suffix != null)
                {
                    spanInlines.Add(suffix);
                }
            }
        }

        private static Hyperlink CreateHyperlink(string url)
        {
            var separator = "://";
            var indexOfSeparator = url.IndexOf(separator, StringComparison.Ordinal);
            var hyperlinkTextContent = url.Substring(indexOfSeparator + separator.Length);

            var hyperlink = new Hyperlink(new Run(hyperlinkTextContent));
            hyperlink.NavigateUri = new Uri(url);
            hyperlink.RequestNavigate += (sender, e) => { Api.Client.Core.OpenWebPage(e.Uri); };
            return hyperlink;
        }

        private static Hyperlink CreateHyperlinkForUrl(Span span, string url)
        {
            if (url.IndexOf(":/", StringComparison.Ordinal) < 0)
            {
                // url doesn't has a protocol prefix - fallback to http
                url = "http://" + url;
            }

            var hyperlink = new Hyperlink(span);
            hyperlink.NavigateUri = new Uri(url);
            hyperlink.RequestNavigate += (sender, e) => { Api.Client.Core.OpenWebPage(e.Uri); };
            return hyperlink;
        }

        private static Hyperlink ExtractHyperlink(string str, int indexOfHttp, out int urlLength)
        {
            var indexOfEnd = str.IndexOfAny(EndUrlSeparators, startIndex: indexOfHttp);
            if (indexOfEnd == -1)
            {
                indexOfEnd = str.Length;
            }

            urlLength = indexOfEnd - indexOfHttp;
            var url = str.Substring(indexOfHttp, urlLength);
            return CreateHyperlink(url);
        }

        private static string ReflowText(string text)
        {
            var rawText = text.Replace("\r", "");
            {
                // re-flow text - trim spaces on each line
                var split = rawText.Split('\n');
                for (var index = 0; index < split.Length; index++)
                {
                    var line = split[index];
                    line = line.Trim();
                    split[index] = line;
                }

                rawText = string.Join(" ", split);
            }
            return rawText;
        }
    }
}