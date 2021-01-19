namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System.Collections.Generic;
    using System.Text;

    public static class ClientListFormatHelper
    {
        // used to separate entries, only if there are more than two entries in the list, doesn't apply to the two last entries
        public const string Format_ListEntrySeparator = ", ";

        // used only if there are more than two entries in the list to format the last entry
        public const string Format_LongListLastEntry_Format = ", and {0}";

        // used only if there are two entries in the list
        public const string Format_TwoEntries_Format = "{0} and {1}";

        public static string Format(IReadOnlyList<string> collection)
        {
            switch (collection.Count)
            {
                case 1: // single entry
                    return collection[0];

                case 2: // two entries
                    return string.Format(Format_TwoEntries_Format, collection[0], collection[1]);
            }

            // three or more entries
            var sb = new StringBuilder();
            for (var index = 0; index < collection.Count - 1; index++)
            {
                sb.Append(collection[index]);
                if (index != collection.Count - 2)
                {
                    sb.Append(Format_ListEntrySeparator);
                }
            }

            sb.AppendFormat(Format_LongListLastEntry_Format,
                            collection[collection.Count - 1]);
            return sb.ToString();
        }
    }
}