namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientLanguages;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class ClientTextTagFormatter
    {
        private static readonly Color ColorH1 = Api.Client.UI.GetApplicationResource<Color>("Color7");

        private static readonly char[] EndUrlSeparators = { ' ', '\r', '\n' };

        public static FormattedTextBlock NewFormattedTextBlock(string text)
        {
            return new() { Content = text };
        }

        public static List<Inline> ParseInlines(string text)
        {
            try
            {
                var result = new List<Inline>();
                var rawText = ReflowText(text);

                foreach (var inline in BuildInlines(rawText))
                {
                    if (inline is not null)
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

            bool modeIsBold = false,
                 modeIsItalic = false,
                 modeIsStrikethrough = false,
                 modeIsUnderline = false,
                 modeIsHeader1 = false;
            string modeUrlCurrentUrl = null;
            Color? modeColor = null;

            for (position = 0; position < text.Length; position++)
            {
                var ch = text[position];
                switch (ch)
                {
                    // escaped bracket
                    case '\\' when position + 1 < text.Length
                                   && (text[position + 1] == '['
                                       || text[position + 1] == ']'):
                        text = text.Remove(position, 1);
                        // do not offset position here despite removing a character
                        break;

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
                            if (modeUrlCurrentUrl is not null)
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
                        else if (tag.StartsWith("color", StringComparison.Ordinal))
                        {
                            var colorStr = tag.Substring(tag.IndexOf("=#", StringComparison.Ordinal) + 2);
                            modeColor = TryParseColor(colorStr);
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

                                case "s":
                                    yield return TryBuildRun();

                                    modeIsStrikethrough = true;
                                    break;

                                case "/s":
                                    yield return TryBuildRun();

                                    modeIsStrikethrough = false;
                                    break;

                                case "u":
                                    yield return TryBuildRun();

                                    modeIsUnderline = true;
                                    break;

                                case "/u":
                                    yield return TryBuildRun();

                                    modeIsUnderline = false;
                                    break;

                                case "h1":
                                    yield return TryBuildRun();

                                    modeIsHeader1 = true;
                                    modeIsBold = true;
                                    modeColor = ColorH1;
                                    break;

                                case "/h1":
                                    yield return TryBuildRun();

                                    modeIsHeader1 = false;
                                    modeIsBold = false;
                                    modeColor = null;
                                    break;

                                case "/url":
                                    yield return TryBuildRun();

                                    modeUrlCurrentUrl = null;
                                    break;

                                case "/color":
                                    yield return TryBuildRun();

                                    modeColor = null;
                                    break;

                                // add bullet point
                                case "*":
                                    yield return TryBuildRun();

                                    if (position > "[*]".Length)
                                    {
                                        // auto-insert empty line
                                        yield return new LineBreak();
                                    }

                                    var bulletPoint = new Run("\u2022") { FontWeight = FontWeights.Bold };
                                    if (modeColor.HasValue)
                                    {
                                        bulletPoint.Foreground = new SolidColorBrush(modeColor.Value);
                                    }

                                    yield return bulletPoint;

                                    // check next char
                                    if (position + 1 < text.Length)
                                    {
                                        if (text[position + 1] == ' ')
                                        {
                                            // skip empty space after bullet point
                                            position++;
                                        }

                                        // add non-breaking space char
                                        yield return new Run("\u00A0");
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

                if (modeIsUnderline
                    && ClientLanguagesManager.CurrentLanguageDefinition.IsFontUnderlineEnabled)
                {
                    // TODO: due to the scan-lines effect in the DataLog window the underline might be invisible
                    // currently there is no way of regulating the underline thickness
                    // it's also impossible to create custom text decorations as TextDecorations in Noesis is an enum.
                    span.TextDecorations = TextDecorations.Underline;
                }

                if (modeIsStrikethrough)
                {
                    // it's actually not yet supported by NoesisGUI
                    span.TextDecorations = TextDecorations.Strikethrough;
                }

                if (modeIsHeader1)
                {
                    span.SetBinding(TextElement.FontSizeProperty,
                                    new Binding()
                                    {
                                        Converter = FloatMultiplierConverter.Instance,
                                        ConverterParameter = 1.2,
                                        Path = new PropertyPath(Control.FontSizeProperty.Name),
                                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor,
                                                                            typeof(TextBlock),
                                                                            1)
                                    });
                }

                if (modeColor.HasValue)
                {
                    span.Foreground = new SolidColorBrush(modeColor.Value);
                }

                var spanInlines = span.Inlines;

                if (modeUrlCurrentUrl is not null)
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
                if (prefix is not null)
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
                if (suffix is not null)
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

                rawText = string.Join("", split);
            }

            if (ClientLanguagesManager.CurrentLanguageDefinition.IsUseCharWrapping)
            {
                // replace all spaces to no-break spaces
                rawText = rawText.Replace(' ', '\u00A0');
            }

            return rawText;
        }

        private static Color TryParseColor(string str)
        {
            if (str.Length != 6)
            {
                Api.Logger.Error("Incorrect color tag: " + str);
                return Colors.White;
            }

            try
            {
                return Color.FromArgb(a: byte.MaxValue,
                                      r: byte.Parse(str.Substring(0, 2), NumberStyles.HexNumber),
                                      g: byte.Parse(str.Substring(2, 2), NumberStyles.HexNumber),
                                      b: byte.Parse(str.Substring(4, 2), NumberStyles.HexNumber));
            }
            catch
            {
                Api.Logger.Error("Incorrect color tag: " + str);
                return Colors.White;
            }
        }
    }
}