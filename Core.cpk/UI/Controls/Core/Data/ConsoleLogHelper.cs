namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Logging;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    internal static class ConsoleLogHelper
    {
        private const string LogTimeFormat = "HH:mm:ss.ffff";

        private static readonly ICoreClientService ClientCore = Api.Client.Core;

        private static SolidColorBrush brushDebug;

        private static SolidColorBrush brushDev;

        private static SolidColorBrush brushError;

        private static SolidColorBrush brushImportant;

        private static SolidColorBrush brushInfo;

        private static SolidColorBrush brushWarning;

        public static ViewModelLogEntry CreateViewModelLogEntry(LogEntry logEntry, bool isFromServer)
        {
            var message = logEntry.Message;
            var foregroundBrush = GetLogSeverityBrush(logEntry.Severity);

            var toolTipText = "Logged at: " + logEntry.Date.ToString(LogTimeFormat);

            var parsedSourceAttribute = ClientCore.ParseLogEntrySource(message);
            if (parsedSourceAttribute.HasValue)
            {
                // successfully parsed
                var source = parsedSourceAttribute.Value;

                var capacity = 40 + source.StackTrace?.Length + source.MethodName?.Length;
                var extraInfoStringBuilder = new StringBuilder(capacity ?? 0);

                var hasMethodName = source.MethodName is not null;
                if (hasMethodName)
                {
                    extraInfoStringBuilder.Append("Method: ");
                    extraInfoStringBuilder.Append(source.MethodName);
                }

                if (source.StackTrace is not null)
                {
                    if (hasMethodName)
                    {
                        extraInfoStringBuilder.AppendLine();
                    }

                    extraInfoStringBuilder.AppendLine("Stack trace: ");
                    extraInfoStringBuilder.Append(source.StackTrace);
                }

                toolTipText = string.Format(
                    "{0}\nScript: {1} line: {2}{3}",
                    toolTipText,
                    source.LocalFilePath ?? "<no file>",
                    source.Line ?? "<no line>",
                    extraInfoStringBuilder.Length > 0
                        ? "\n" + extraInfoStringBuilder
                        : string.Empty);
                message = source.CleanMessage;
            }

            BaseCommand commandOpenFile;
            if (parsedSourceAttribute.HasValue
                && parsedSourceAttribute.Value.RealFilePath is not null
                && parsedSourceAttribute.Value.Line is not null)
            {
                var source = parsedSourceAttribute.Value;
                commandOpenFile =
                    new ActionCommand(() => ExecuteCommandOpenSourceFile(source.RealFilePath, source.Line));
            }
            else
            {
                commandOpenFile = null;
            }

            return new ViewModelLogEntry(message, toolTipText, foregroundBrush, commandOpenFile, isFromServer);
        }

        public static void InitBrushes(FrameworkElement frameworkElement)
        {
            if (brushDebug is not null)
            {
                return;
            }

            brushDebug = frameworkElement.GetResource<SolidColorBrush>("BrushDebug");
            brushInfo = frameworkElement.GetResource<SolidColorBrush>("BrushInfo");
            brushImportant = frameworkElement.GetResource<SolidColorBrush>("BrushImportant");
            brushWarning = frameworkElement.GetResource<SolidColorBrush>("BrushWarning");
            brushError = frameworkElement.GetResource<SolidColorBrush>("BrushError");
            brushDev = frameworkElement.GetResource<SolidColorBrush>("BrushDev");
        }

        private static void ExecuteCommandOpenSourceFile(string filePath, string lineString)
        {
            ClientCore.OpenFileEditor(filePath, int.Parse(lineString));
        }

        private static Brush GetLogSeverityBrush(LogSeverity logSeverity)
        {
            switch (logSeverity)
            {
                case LogSeverity.Debug:
                    return brushDebug;

                case LogSeverity.Info:
                    return brushInfo;

                case LogSeverity.Important:
                    return brushImportant;

                case LogSeverity.Warning:
                    return brushWarning;

                case LogSeverity.Error:
                    return brushError;

                case LogSeverity.Dev:
                    return brushDev;

                default:
                    throw new ArgumentException("Unknown log severity: " + logSeverity);
            }
        }
    }
}