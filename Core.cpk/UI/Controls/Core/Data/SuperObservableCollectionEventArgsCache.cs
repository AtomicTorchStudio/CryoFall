namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class SuperObservableCollectionEventArgsCache
    {
        public static readonly PropertyChangedEventArgs EventArgsCountPropertyChanged
            = new("Count");

        public static readonly PropertyChangedEventArgs EventArgsIndexerPropertyChanged
            = new("Item[]");

        public static readonly NotifyCollectionChangedEventArgs EventArgsResetCollection
            = new(NotifyCollectionChangedAction.Reset);
    }
}