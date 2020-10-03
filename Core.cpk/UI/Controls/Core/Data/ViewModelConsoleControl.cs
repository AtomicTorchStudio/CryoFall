namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ViewModelConsoleControl : BaseViewModel
    {
        private readonly Action onFilterChanged;

        private readonly IClientStorage sessionStorageFilterString;

        private bool displaySeverityDebug = true;

        private bool displaySeverityDev = true;

        private bool displaySeverityError = true;

        private bool displaySeverityImportant = true;

        private bool displaySeverityInfo = true;

        private bool displaySeverityWarning = true;

        private string filterString;

        private SuperObservableCollection<ViewModelLogEntry> logEntriesCollection =
            new SuperObservableCollection<ViewModelLogEntry>();

        private string suggestionsListPaddingText;

        /// <summary>
        /// This constructor is only for WPF design-time
        /// </summary>
        public ViewModelConsoleControl()
        {
            this.SuggestionsListVisibility = Visibility.Visible;
            this.SuggestionsListItems.Add("Test item 1");
            this.SuggestionsListItems.Add("Test item 2");
            this.SuggestionsListItems.Add("Test item 3");
        }

        public ViewModelConsoleControl(Action onFilterChanged)
        {
            this.onFilterChanged = onFilterChanged;

            this.sessionStorageFilterString = Api.Client.Storage.GetSessionStorage(
                $"{nameof(ViewModelConsoleControl)}.{nameof(this.FilterText)}");
            this.sessionStorageFilterString.TryLoad(out this.filterString);
        }

        public bool DisplaySeverityDebug
        {
            get => this.displaySeverityDebug;
            set
            {
                this.displaySeverityDebug = value;
                this.NotifyThisPropertyChanged();
                this.InvokeFilterChanged();
            }
        }

        public bool DisplaySeverityDev
        {
            get => this.displaySeverityDev;
            set
            {
                if (value == this.displaySeverityDev)
                {
                    return;
                }

                this.displaySeverityDev = value;
                this.NotifyThisPropertyChanged();
                this.InvokeFilterChanged();
            }
        }

        public bool DisplaySeverityError
        {
            get => this.displaySeverityError;
            set
            {
                if (value == this.displaySeverityError)
                {
                    return;
                }

                this.displaySeverityError = value;
                this.NotifyThisPropertyChanged();
                this.InvokeFilterChanged();
            }
        }

        public bool DisplaySeverityImportant
        {
            get => this.displaySeverityImportant;
            set
            {
                if (value == this.displaySeverityImportant)
                {
                    return;
                }

                this.displaySeverityImportant = value;
                this.NotifyThisPropertyChanged();
                this.InvokeFilterChanged();
            }
        }

        public bool DisplaySeverityInfo
        {
            get => this.displaySeverityInfo;
            set
            {
                if (value == this.displaySeverityInfo)
                {
                    return;
                }

                this.displaySeverityInfo = value;
                this.NotifyThisPropertyChanged();
                this.InvokeFilterChanged();
            }
        }

        public bool DisplaySeverityWarning
        {
            get => this.displaySeverityWarning;
            set
            {
                if (value == this.displaySeverityWarning)
                {
                    return;
                }

                this.displaySeverityWarning = value;
                this.NotifyThisPropertyChanged();
                this.InvokeFilterChanged();
            }
        }

        public string FilterText
        {
            get => this.filterString ?? string.Empty;
            set
            {
                value ??= string.Empty;
                if (value == this.filterString)
                {
                    return;
                }

                this.filterString = value;
                this.NotifyThisPropertyChanged();
                this.InvokeFilterChanged();

                this.sessionStorageFilterString.Save(this.filterString, writeToLog: false);
            }
        }

        public SuperObservableCollection<ViewModelLogEntry> LogEntriesCollection
        {
            get => this.logEntriesCollection;
            set
            {
                if (this.logEntriesCollection == value)
                {
                    return;
                }

                this.logEntriesCollection = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public Visibility ServerLogVisibility { get; } = Api.IsEditor ? Visibility.Visible : Visibility.Collapsed;

        public SuperObservableCollection<string> SuggestionsListItems { get; }
            = new SuperObservableCollection<string>();

        public string SuggestionsListPaddingText
        {
            get => this.suggestionsListPaddingText;
            private set
            {
                this.suggestionsListPaddingText = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public int SuggestionsListSelectedItemIndex { get; set; }

        public Visibility SuggestionsListVisibility { get; private set; }

        public void SetSuggestions(IReadOnlyList<string> suggestions)
        {
            if (suggestions is null
                || suggestions.Count == 0)
            {
                this.SuggestionsListItems.Clear();
                this.SuggestionsListVisibility = Visibility.Collapsed;
                return;
            }

            this.SuggestionsListItems.ClearAndAddRange(suggestions);
            this.SuggestionsListSelectedItemIndex = 0;
            this.SuggestionsListVisibility = Visibility.Visible;
        }

        public void SetSuggestionsControlOffset(int charsOffset)
        {
            this.SuggestionsListPaddingText = new string(' ', count: charsOffset);
        }

        private void InvokeFilterChanged()
        {
            this.onFilterChanged?.Invoke();
        }
    }
}