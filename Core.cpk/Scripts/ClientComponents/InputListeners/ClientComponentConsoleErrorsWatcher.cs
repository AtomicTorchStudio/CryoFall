namespace AtomicTorch.CBND.CoreMod.ClientComponents.InputListeners
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Logging;

    public class ClientComponentConsoleErrorsWatcher : ClientComponent
    {
        private ILogEntriesProvider currentProvider;

        protected override void OnDisable()
        {
            this.SetLogEntriesProvider(null);
        }

        protected override void OnEnable()
        {
            this.SetLogEntriesProvider(Logger.LogEntriesProvider);
        }

        private static LogOverlayControl GetControl()
        {
            if (ConsoleControl.Instance is not null
                && ConsoleControl.Instance.IsDisplayed)
            {
                // error will be logged into the console
                return null;
            }

            var instance = LogOverlayControl.Instance;
            if (instance is not null)
            {
                return instance;
            }

            instance = new LogOverlayControl();
            Api.Client.UI.LayoutRootChildren.Add(instance);
            return instance;
        }

        private static bool IsWatchedSeverity(LogSeverity severity)
        {
            return severity == LogSeverity.Error
                   || severity == LogSeverity.Dev;
        }

        private void Display(LogEntryWithOrigin logEntry)
        {
            GetControl()?.Display(logEntry);
        }

        private void LogEntriesProviderNewLogEntryHandler(LogEntryWithOrigin logEntry)
        {
            if (!IsWatchedSeverity(logEntry.LogEntry.Severity))
            {
                return;
            }

            this.currentProvider.LastProcessedErrorDate = DateTime.Now;
            this.Display(logEntry);
        }

        private void SetLogEntriesProvider(ILogEntriesProvider newProvider)
        {
            if (this.currentProvider is not null)
            {
                this.currentProvider.NewLogEntry -= this.LogEntriesProviderNewLogEntryHandler;
            }

            this.currentProvider = newProvider;

            if (this.currentProvider is null)
            {
                return;
            }

            this.currentProvider.NewLogEntry += this.LogEntriesProviderNewLogEntryHandler;

            var logOverlayControl = GetControl();
            if (logOverlayControl is null)
            {
                return;
            }

            var errors = (from l in this.currentProvider.Log.TakeLastNetStandard(1000)
                          let logEntry = l.LogEntry
                          where IsWatchedSeverity(logEntry.Severity)
                                && logEntry.Date > this.currentProvider.LastProcessedErrorDate
                          select l).ToList();

            this.currentProvider.LastProcessedErrorDate = DateTime.Now;

            logOverlayControl.Clear();
            logOverlayControl.Display(errors);
        }
    }
}