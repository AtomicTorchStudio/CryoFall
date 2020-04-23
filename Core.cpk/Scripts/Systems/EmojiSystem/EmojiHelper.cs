namespace AtomicTorch.CBND.CoreMod.Systems.EmojiSystem
{
    using System.Collections.Generic;
    using System.Text;

    internal static class EmojiHelper
    {
        // partially based on https://stackoverflow.com/a/33516763
        private static readonly Dictionary<string, string> Dictionary
            = new Dictionary<string, string>
            {
                { ":(", "😦" },
                { ":)", "😃" },
                { ",:(", "😓" },
                { ",:)", "😅" },
                { ",:-(", "😓" },
                { ",:-)", "😅" },
                { "0:)", "😇" },
                { "0:-)", "😇" },
                { "8-)", "😎" },
                { ":\")", "😊" },
                { ":$", "😒" },
                { ":'(", "😢" },
                { ":')", "😂" },
                { ":'-(", "😢" },
                { ":'-)", "😂" },
                { ":'-D", "😂" },
                { ":'D", "😂" },
                { ":*", "😗" },
                { ":,'(", "😭" },
                { ":,'-(", "😭" },
                { ":,(", "😢" },
                { ":,)", "😂" },
                { ":,-(", "😢" },
                { ":,-)", "😂" },
                { ":,-D", "😂" },
                { ":,D", "😂" },
                { ":-\"):", "😊" },
                { ":-$", "😒" },
                { ":-(", "😦" },
                { ":-)", "😃" },
                { ":-*", "😗" },
                { ":-/", "😕" },
                { ":-@", "😡" },
                { ":-D", "😄" },
                { ":-o", "😮" },
                { ":-O", "😮" },
                { ":-P", "😛" },
                { ":-S", "😒" },
                { ":-Z", "😒" },
                { ":/", "😕" },
                { ":@", "😡" },
                { ":D", "😄" },
                { ":o", "😮" },
                { ":O", "😮" },
                { ":P", "😛" },
                { ":s", "😒" },
                { ":z", "😒" },
                { ":|", "😐" },
                { ":-|", "😐" },
                { ";(", "😭" },
                { ";)", "😉" },
                { ";-(", "😭" },
                { ";-)", "😉" },
                { "]:)", "😈" },
                { "]:-)", "😈" },
                { "B-)", "😎" },
                { "o:)", "😇" },
                { "O:)", "😇" },
                { "O:-)", "😇" },
                { "o:-)", "😇" },
                { "X-)", "😆" },
                { "x-)", "😆" },
                { ":rofl:", "🤣" }
            };

        static EmojiHelper()
        {
            // partially based on 
        }

        public static string ReplaceAsciiToUnicodeEmoji(string text)
        {
            var sb = new StringBuilder(text);
            foreach (var entry in Dictionary)
            {
                sb.Replace(entry.Key, entry.Value);
            }

            return sb.ToString();
        }
    }
}