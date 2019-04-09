namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    internal static class SuperObservableCollectionEventArgsCache
    {
        public static readonly PropertyChangedEventArgs EventArgsCountPropertyChanged
            = new PropertyChangedEventArgs("Count");

        public static readonly PropertyChangedEventArgs EventArgsIndexerPropertyChanged
            = new PropertyChangedEventArgs("Item[]");

        public static readonly NotifyCollectionChangedEventArgs EventArgsResetCollection
            = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
    }
}